using ICSharpCode.SharpZipLib.Zip;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace ePOS3.Utils
{
    public class CryptorEngine
    {
        private static string SPECIAL = "&amp,&nbsp;";

        public static string GetXml(string url)
        {
            using (XmlReader xr = XmlReader.Create(url, new XmlReaderSettings() { IgnoreWhitespace = true }))
            {
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter xw = XmlWriter.Create(sw))
                    {
                        xw.WriteNode(xr, false);
                    }
                    return sw.ToString();
                }
            }
        }
          
        public static string Encrypt_RSA(string data, int key_size, string publickey, int byte_encrypt)
        {
            data = data.Trim();
            var rsa = new RSACryptoServiceProvider(key_size);
            rsa.FromXmlString(GetXml(publickey));   
            return Convert.ToBase64String(encrypt(rsa, Encoding.UTF8.GetBytes(data), byte_encrypt));
        }

        private static byte[] encrypt(RSACryptoServiceProvider rsa, byte[] bytes, int byte_encrypt)
        {
            byte[] scrambled = new byte[0];
            byte[] toReturn = new byte[0];
            byte[] buffer = new byte[byte_encrypt];
            for (int i = 0; i < bytes.Length; i++)
            {
                if ((i > 0) && (i % byte_encrypt == 0))
                {
                    scrambled = rsa.Encrypt(buffer, false);
                    toReturn = append(toReturn, scrambled);
                    buffer = new byte[byte_encrypt];
                }
                buffer[i % byte_encrypt] = bytes[i];
            }
            scrambled = rsa.Encrypt(buffer, false);
            toReturn = append(toReturn, scrambled);
            return toReturn;
        }

        private static byte[] append(byte[] prefix, byte[] suffix)
        {
            byte[] toReturn = new byte[prefix.Length + suffix.Length];
            for (int i = 0; i < prefix.Length; i++)
            {
                toReturn[i] = prefix[i];
            }
            for (int i = 0; i < suffix.Length; i++)
            {
                toReturn[i + prefix.Length] = suffix[i];
            }
            return toReturn;
        }

        public static string Decrypt_RSA(string data, int key_size, string privateKey, int byte_decrypt)
        {
            data = data.Trim();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(key_size);
            rsa.FromXmlString(privateKey);
            return del_special(Encoding.UTF8.GetString(trim(decrypt(rsa, Convert.FromBase64String(data), byte_decrypt))));
        }

        private static string del_special(string data)
        {
            string[] Array = SPECIAL.Replace(",", " ").Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
            string result = data;
            for (int i = 0; i < Array.Length; i++)
            {
                result = result.Replace(Array.ElementAt(i).ToString(), "");
            }
            return result;
        }

        private static byte[] trim(byte[] bytes)
        {
            int i = bytes.Length - 1;
            while (i >= 0 && bytes[i] == 0)
            {
                --i;
            }
            byte[] buffer = new byte[i + 1];
            Array.Copy(bytes, buffer, i + 1);
            return buffer;
        }

        private static byte[] decrypt(RSACryptoServiceProvider rsa, byte[] bytes, int byte_decrypt)
        {
            byte[] scrambled = new byte[0];
            byte[] toReturn = new byte[0];
            byte[] buffer = new byte[byte_decrypt];

            for (int i = 0; i < bytes.Length; i++)
            {
                if ((i > 0) && (i % byte_decrypt == 0))
                {
                    scrambled = rsa.Decrypt(buffer, false);
                    toReturn = append(toReturn, scrambled);
                    buffer = new byte[byte_decrypt];
                }
                buffer[i % byte_decrypt] = bytes[i];
            }
            scrambled = rsa.Decrypt(buffer, false);
            toReturn = append(toReturn, scrambled);
            return toReturn;
        }

        public static string Encrypt(string toEncrypt, bool useHashing, string privateKey, string publicKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string key = getKEY(privateKey, publicKey);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cipherString, bool useHashing, string privateKey, string publicKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            string key = getKEY(privateKey, publicKey);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string MD5(string cleanString)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] clearBytes = utf8.GetBytes(cleanString);
            Byte[] hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);
            return (BitConverter.ToString(hashedBytes)).Replace("-", "");

        }

        public static string convertToUnSign(string str)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = str.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string Encrypt_CBC(string toEncrypt, bool useHashing, string privateKey, string publicKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            string key = getKEY(privateKey, publicKey);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.None;
            tdes.IV = Encoding.UTF8.GetBytes(getIV(privateKey, publicKey));
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return ByteArrayToString(resultArray);
        }

        public static string Decrypt_CBC(string cipherString, bool useHashing, string privateKey, string publicKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = HexStringToByte(cipherString);
            string key = getKEY(privateKey, publicKey);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.None;
            tdes.IV = Encoding.UTF8.GetBytes(getIV(privateKey, publicKey));
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static byte[] HexStringToByte(string hexString)
        {
            try
            {
                int bytesCount = (hexString.Length) / 2;
                byte[] bytes = new byte[bytesCount];
                for (int x = 0; x < bytesCount; ++x)
                    bytes[x] = Convert.ToByte(hexString.Substring(x * 2, 2), 16);
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString().ToUpper();
        }

        public static string Validate_Pass(string pass)
        {
            int i_length = pass.Length;
            int mod = i_length % 8;
            if (mod > 0)
                for (int i = 0; i < (8 - mod); i++)
                    pass = pass + " ";
            return pass;
        }

        public static String getIV(string privateKey, string publicKey)
        {
            int length1 = privateKey.Length;
            int length2 = publicKey.Length;
            return privateKey.Substring((length1 - 4) / 2, 4) + publicKey.Substring((length2 - 4) / 2, 4);
        }

        public static String getKEY(string privateKey, string publicKey)
        {
            int length1 = privateKey.Length;
            int length2 = publicKey.Length;
            return privateKey.Substring((length1 - 12) / 2, 12) + publicKey.Substring((length2 - 12) / 2, 12);
        }
    }

    public class CompressUtil
    {
        public static string DecryptBase64(string s, bool autoConvert = true)
        {
            try
            {               
                s = Regex.Replace(s.Replace(Environment.NewLine, ""), @"(\r\n?|\n)", "");
                bool is64 = (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
                if (is64)
                {
                    var dataAll = ExtractTextFromBase64Gzip(s);
                    return dataAll;
                }
                return s;
            }
            catch (Exception ex)
            {                
                Logging.EPOSLogger.Error("DetectIfBase64: " + ex.Message);
                return s;
            }
        }

        public static string ExtractTextFromBase64Gzip(string base64)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                return ExtractTextFromGzip(bytes);
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.ErrorFormat("ExtractTextFromBase64Gzip : {0}", ex.Message);
            }
            return "";
        }


        public static string ExtractTextFromGzip(byte[] compressBytes)
        {
            try
            {

                return GetFullContentWithoutPassUsingGzip(compressBytes);
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.ErrorFormat("ExtractTextFromGzip : {0}", ex.Message);
                return "ERROR";

            }
        }

        public static string GetFullContentWithoutPassUsingGzip(byte[] dataFileBytes)
        {
            try
            {
                string content = DecompressToTextNoPassUsingGzip(dataFileBytes);
                return content;
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error("GetFullContentWithoutPass: " + ex.Message);
                return "";
            }
        }
        
        private static string DecompressToTextNoPassUsingGzip(byte[] fileData)
        {
            string errorSummary = "";
            try
            {
                errorSummary += $"Length:{fileData}";
                var data = GetUncompressedGZipBytes(fileData);
                errorSummary += $"Length:{fileData},data{data.Length}";
                var result = BytesToText(data);
                return result;
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error($"{MethodBase.GetCurrentMethod().Name}" + ex.Message);
                return "";
            }
        }

        public string CompressToTextNoPassUsingGzip(string dataBase64)
        {
            try
            {
                //dataBase64 = dataBase64.Replace("\r\n", "");
                byte[] compressedBytes;

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(dataBase64)))
                {
                    compressedBytes = Compress(stream);
                }

                string result = Convert.ToBase64String(compressedBytes);
                return result;
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error("CompressToTextNoPassUsingGzip: " + ex.Message);
                return dataBase64;
            }
        }
        
        public static byte[] GetUncompressedGZipBytes(byte[] gZipData)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            try
            {
                using (GZipStream stream = new GZipStream(new MemoryStream(gZipData), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (MemoryStream memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        } while (count > 0);
                        return memory.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error(
                    $"GetUncompressedGZipBytes: Size(Mb){gZipData.Length / (1024 * 1024)}" + ex.Message);
                return new byte[] { };
            }

        }
        
        private byte[] GetUncompressedZipBytes(byte[] data)
        {
            try
            {
                using (var outputStream = new MemoryStream())
                using (var inputStream = new MemoryStream(data))
                {
                    using (var zipInputStream = new ZipInputStream(inputStream))
                    {
                        zipInputStream.GetNextEntry();
                        zipInputStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error(
                    $"GetUncompressedZipBytes: Size(Mb){data.Length / (1024 * 1024)} length:{data.Length}" + ex.Message);
                return new byte[] { };
            }
        }
        
        public static byte[] Compress(Stream input)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                input.CopyTo(zipStream);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static string BytesToText(byte[] datafiles)
        {
            try
            {
                string str = (new UTF8Encoding()).GetString(datafiles);
                return str;
            }
            catch (Exception ex)
            {
                Logging.EPOSLogger.Error($"BytesToText{ex.Message}");
            }
            return "";
        }


    }

    public class EncryptionClass
    {
        public AsymmetricCipherKeyPair Keys { get; private set; }
        public static string privateKey { get; set; }
        public static string publicKey { get; set; }

        private readonly Pkcs1Encoding _engine;


        public EncryptionClass()
        {
            //Keys = GenerateKeys();
            _engine = new Pkcs1Encoding(new RsaEngine());
        }

        public static string getPrivateKey()
        {
            try
            {
                var reader = File.OpenText(@"data\privateKey.pem");
                privateKey = reader.ReadToEnd();
            }
            catch
            {
            }

            return privateKey;
        }

        public static string getPublicKey()
        {
            try
            {
                var reader = File.OpenText(@"data\publicKey.pem");
                publicKey = reader.ReadToEnd();
            }
            catch
            {
            }

            return publicKey;
        }

        public static string RsaEncryptWithPublic(string clearText, string publicKey)
        {
            try
            {
                var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

                var encryptEngine = new Pkcs1Encoding(new RsaEngine());
                //var encryptEngine = new RsaEngine();

                using (var txtreader = File.OpenText(publicKey))
                {
                    var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                    encryptEngine.Init(true, keyParameter);
                }

                var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
                return encrypted;
            }
            catch (Exception ex)
            {
                return "";
                //Logging.ECASHLogger.ErrorFormat("EncryptionClass RsaEncryptWithPublic: clearText: {0} - publicKey: {1} - ex: {2}", clearText, publicKey, ex.Message);
            }


        }

        public static string RsaEncryptWithBodyPublicKey(string clearText, string publicKey)
        {
            try
            {
                var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

                var encryptEngine = new Pkcs1Encoding(new RsaEngine());
                //var encryptEngine = new RsaEngine();

                using (var txtreader = new StringReader(publicKey))
                {
                    var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();

                    encryptEngine.Init(true, keyParameter);
                }

                var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
                return encrypted;
            }
            catch (Exception ex)
            {
                return "";
                //Logging.ECASHLogger.ErrorFormat("EncryptionClass RsaEncryptWithPublic: clearText: {0} - publicKey: {1} - ex: {2}", clearText, publicKey, ex.Message);
            }


        }

        public static byte[] Sign(byte[] hash, string signerAlgorithm, string hashAlgorithmOid, AsymmetricKeyParameter privateSigningKey)
        {

            var digestAlgorithm = new AlgorithmIdentifier(new DerObjectIdentifier(hashAlgorithmOid), DerNull.Instance);
            var dInfo = new DigestInfo(digestAlgorithm, hash);
            byte[] digest = dInfo.GetDerEncoded();

            ISigner signer = SignerUtilities.GetSigner(signerAlgorithm);
            signer.Init(true, privateSigningKey);
            signer.BlockUpdate(digest, 0, digest.Length);
            byte[] signature = signer.GenerateSignature();
            return signature;
        }

        public static string RsaDataSign(string clearText, string privateKey, string pw)
        {
            try
            {
                var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);
                ISigner signer = SignerUtilities.GetSigner("MD5withRSA");
                using (var txtreader = File.OpenText(privateKey))
                {
                    PemReader pemReader = new PemReader(txtreader, new PasswordFinder(pw));
                    object privateKeyObject = pemReader.ReadObject();
                    RsaPrivateCrtKeyParameters rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
                    RsaKeyParameters rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
                    var keyPair = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);

                    //var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader, new PasswordFinder(pw)).ReadObject();

                    signer.Init(true, keyPair.Private);


                    //var encryptEngine = new Pkcs1Encoding(new RsaBlindedEngine());
                    //encryptEngine.Init(true, keyPair.Private);

                    //DigestInfo dInfo = new DigestInfo(new AlgorithmIdentifier(X509ObjectIdentifiers.IdSha1, DerNull.Instance), bytesToEncrypt);
                    //byte[] digestInfo = dInfo.GetEncoded(Asn1Encodable.Der);

                    //signer.BlockUpdate(digestInfo, 0, digestInfo.Length);
                    //var zzz = Convert.ToBase64String(signer.GenerateSignature());

                }

                signer.BlockUpdate(bytesToEncrypt, 0, bytesToEncrypt.Length);
                var encrypted = Convert.ToBase64String(signer.GenerateSignature());



                return encrypted;
            }
            catch (Exception ex)
            {
                return "";
                //Logging.ECASHLogger.ErrorFormat("EncryptionClass RsaEncryptWithPrivate: clearText: {0} - publicKey: {1} - ex: {2}", clearText, privateKey, ex.Message);
            }

        }


        public static string RsaEncryptWithPrivate(string clearText, string privateKey)
        {
            try
            {
                var bytesToEncrypt = Encoding.UTF8.GetBytes(clearText);

                var encryptEngine = new Pkcs1Encoding(new RsaEngine());

               //StreamReader reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(privateKey)));

                using (var txtreader = File.OpenText(privateKey))
                {
                    var keyPair = (AsymmetricCipherKeyPair)new PemReader(txtreader).ReadObject();

                    encryptEngine.Init(true, keyPair.Private);
                }

                var encrypted = Convert.ToBase64String(encryptEngine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length));
                return encrypted;
            }
            catch (Exception ex)
            {
                return "";
                //Logging.ECASHLogger.ErrorFormat("EncryptionClass RsaEncryptWithPrivate: clearText: {0} - publicKey: {1} - ex: {2}", clearText, privateKey, ex.Message);
            }

        }


        // Decryption:
        public static string RsaDecrypt(string base64Input, string privateKey)
        {
            try
            {
                var bytesToDecrypt = Convert.FromBase64String(base64Input);

                //get a stream from the string
                AsymmetricCipherKeyPair keyPair;
                //Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters key;
                var decryptEngine = new Pkcs1Encoding(new RsaEngine());
                //var decryptEngine = new RsaEngine();

                using (var reader = File.OpenText(privateKey))
                {
                    keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
                    // key = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)new PemReader(reader).ReadObject();                    

                    decryptEngine.Init(false, keyPair.Private);
                }

                var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
                return decrypted;
            }
            catch (Exception ex)
            {
                return "";
                //Logging.ECASHLogger.ErrorFormat("EncryptionClass RsaDecrypt: base64Input: {0} - privateKey: {1} - ex: {2}", base64Input, privateKey, ex.Message);
            }
        }

        public static AsymmetricCipherKeyPair GenerateKeys(int keySize = 1024)
        {
            var rsaKeyParams = new RsaKeyGenerationParameters(BigInteger.ProbablePrime(keySize, new Random()),
                                                              new SecureRandom(), keySize, 25); //Unsure about the certinaty parameter
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(rsaKeyParams);

            AsymmetricCipherKeyPair keys = keyGen.GenerateKeyPair();
            AsymmetricKeyParameter private_key = keys.Private;
            AsymmetricKeyParameter public_key = keys.Public;

            //Generate private key
            TextWriter textPrivateWriter = new StringWriter();
            PemWriter pemPrivateWriter = new PemWriter(textPrivateWriter);
            pemPrivateWriter.WriteObject(keys.Private);
            pemPrivateWriter.Writer.Flush();
            privateKey = textPrivateWriter.ToString();

            //Generate public key
            TextWriter textPublicWriter = new StringWriter();
            PemWriter pemPublicWriter = new PemWriter(textPublicWriter);
            pemPublicWriter.WriteObject(keys.Public);
            pemPublicWriter.Writer.Flush();
            publicKey = textPublicWriter.ToString();


            return keys;
        }


        public static string GeneratePrivateKey(string password, int strength, int iterationCount)
        {
            var rsa = new Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator();
            rsa.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), strength));
            var asym = rsa.GenerateKeyPair();
            var generator = new Org.BouncyCastle.OpenSsl.Pkcs8Generator(asym.Private, Org.BouncyCastle.OpenSsl.Pkcs8Generator.PbeSha1_3DES);
            generator.IterationCount = iterationCount;
            generator.Password = password.ToCharArray();
            var pem = generator.Generate();
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(pem);
            pemWriter.Writer.Flush();
            string privateKey = textWriter.ToString();
            return privateKey;

        }

        public static string GeneratePublicKey(int strength)
        {
            var rsa = new Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator();
            rsa.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), strength));
            var asym = rsa.GenerateKeyPair();
            //var generator = new Org.BouncyCastle.OpenSsl.Pkcs8Generator(asym.Public, Org.BouncyCastle.OpenSsl.Pkcs8Generator.PbeSha1_3DES);
            //generator.IterationCount = iterationCount;
            //generator.Password = password.ToCharArray();
            //var pem = generator.Generate();
            //Generate public key
            TextWriter textPublicWriter = new StringWriter();
            PemWriter pemPublicWriter = new PemWriter(textPublicWriter);
            pemPublicWriter.WriteObject(asym.Public);
            pemPublicWriter.Writer.Flush();
            publicKey = textPublicWriter.ToString();
            return publicKey;

        }

        public static string SavePublicKey(byte[] b_publicKey)
        {
            var str_publicKey = string.Empty;
            try
            {
                str_publicKey = "-----BEGIN PUBLIC KEY-----\n\r" + Convert.ToBase64String(b_publicKey) + "\n\r-----END PUBLIC KEY-----";

                using (var txtreader = new StringReader(publicKey))
                {
                    TextWriter textPublicWriter = new StringWriter();
                    PemWriter pemPublicWriter = new PemWriter(textPublicWriter);
                    pemPublicWriter.WriteObject(new PemReader(txtreader).ReadObject());
                    pemPublicWriter.Writer.Flush();
                    str_publicKey = textPublicWriter.ToString();
                }

            }
            catch (Exception ex)
            {
            }

            return str_publicKey;
        }


        public static string GenerateKeysPasswhare(string password, int strength, int iterationCount)
        {
            var rsa = new Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator();
            rsa.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new Org.BouncyCastle.Security.SecureRandom(), strength));
            var keys = rsa.GenerateKeyPair();
            var generator = new Org.BouncyCastle.OpenSsl.Pkcs8Generator(keys.Private, Org.BouncyCastle.OpenSsl.Pkcs8Generator.PbeSha1_3DES);
            generator.IterationCount = iterationCount;
            generator.Password = password.ToCharArray();
            var pem = generator.Generate();

            //Generate private key
            TextWriter textPrivateWriter = new StringWriter();
            PemWriter pemPrivateWriter = new PemWriter(textPrivateWriter);
            pemPrivateWriter.WriteObject(pem);
            pemPrivateWriter.Writer.Flush();
            privateKey = textPrivateWriter.ToString();

            //Generate public key
            TextWriter textPublicWriter = new StringWriter();
            PemWriter pemPublicWriter = new PemWriter(textPublicWriter);
            pemPublicWriter.WriteObject(keys.Public);
            pemPublicWriter.Writer.Flush();
            publicKey = textPublicWriter.ToString();
            return publicKey;

        }

        //public static string DecryptWithPasswhare(string base64Input, string privateKey, string password)
        //{
        //    var bytesToDecrypt = Convert.FromBase64String(base64Input);

        //    //get a stream from the string
        //    AsymmetricCipherKeyPair keyPair;
        //    var decryptEngine = new Pkcs1Encoding(new RsaEngine());

        //    using (var txtreader = File.OpenText(privateKey))
        //    {
        //        PemReader pemReader = new PemReader(txtreader, new PasswordFinder(password));
        //        object privateKeyObject = pemReader.ReadObject();
        //        RsaPrivateCrtKeyParameters rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
        //        RsaKeyParameters rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
        //        keyPair = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);

        //        decryptEngine.Init(false, keyPair.Private);
        //    }

        //    var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));
        //    return decrypted;
        //}

        //private static AsymmetricCipherKeyPair DecodePrivateKey(string encryptedPrivateKey, string password)
        //{
        //    using (var txtreader = File.OpenText(privateKey))
        //    {
        //        PemReader pemReader = new PemReader(txtreader, new PasswordFinder(password));
        //        object privateKeyObject = pemReader.ReadObject();
        //        RsaPrivateCrtKeyParameters rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
        //        RsaKeyParameters rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
        //        AsymmetricCipherKeyPair kp = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);
        //        return kp;
        //    }
        //}


        //public static string GenerateKeysChangePasswhare(string privateKey, string oldpassword, int strength, int iterationCount, string newpassword)
        //{
        //    try
        //    {
        //        //get a stream from the string
        //        AsymmetricCipherKeyPair keyPair;
        //        using (var txtreader = File.OpenText(privateKey))
        //        {
        //            PemReader pemReader = new PemReader(txtreader, new PasswordFinder(oldpassword));
        //            object privateKeyObject = pemReader.ReadObject();
        //            RsaPrivateCrtKeyParameters rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
        //            RsaKeyParameters rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
        //            keyPair = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);

        //            var generator = new Org.BouncyCastle.OpenSsl.Pkcs8Generator(keyPair.Private, Org.BouncyCastle.OpenSsl.Pkcs8Generator.PbeSha1_3DES);
        //            generator.IterationCount = iterationCount;
        //            generator.Password = newpassword.ToCharArray();
        //            var pem = generator.Generate();

        //            //Generate private key
        //            TextWriter textPrivateWriter = new StringWriter();
        //            PemWriter pemPrivateWriter = new PemWriter(textPrivateWriter);
        //            pemPrivateWriter.WriteObject(pem);
        //            pemPrivateWriter.Writer.Flush();
        //            privateKey = textPrivateWriter.ToString();

        //            return privateKey;

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return "";
        //    }


        //}


        //public static bool isAuthorization(string privateKey, string pw)
        //{
        //    try
        //    {
        //        bool isAuthor = false;
        //        ISigner signer = SignerUtilities.GetSigner("MD5withRSA");
        //        using (var txtreader = File.OpenText(privateKey))
        //        {
        //            PemReader pemReader = new PemReader(txtreader, new PasswordFinder(pw));
        //            object privateKeyObject = pemReader.ReadObject();
        //            RsaPrivateCrtKeyParameters rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
        //            RsaKeyParameters rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
        //            var keyPair = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);
        //            signer.Init(true, keyPair.Private);
        //            isAuthor = true;
        //        }


        //        return isAuthor;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }

        //}


        //Demo TuanLA
        public static string RSABouncyEncrypt(string content,string privateKey)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(content);
            AsymmetricKeyParameter keyPair;
            //using (var reader = File.OpenText(@"F:\SVN\eCash\eCash\keys\ecash.pub"))
            using (var reader = File.OpenText(privateKey))
                keyPair = (AsymmetricKeyParameter)new Org.BouncyCastle.OpenSsl.PemReader(reader).ReadObject();

            //var engine = new RsaEngine();
            //engine.Init(false, keyPair);
            //var encrypted = engine.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            //var cryptMessage = Convert.ToBase64String(encrypted);

            IAsymmetricBlockCipher eng = new Pkcs1Encoding(new RsaEngine());
            eng.Init(true, keyPair);
            var encrypted = eng.ProcessBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            var cryptMessage = Convert.ToBase64String(encrypted);

            //Decrypt before return statement to check that it has been encrypted correctly
            //RSABouncyDecrypt(cryptMessage);
            return cryptMessage;
        }

        public static void RSABouncyDecrypt(string string64)
        {
            var bytesToDecrypt = Convert.FromBase64String(string64); // string to decrypt, base64 encoded

            AsymmetricCipherKeyPair keyPair;

            //AsymmetricKeyParameter key = ReadAsymmetricKeyParameter(@"c:\ECPAY\CNTT-ECPAY\Coding\eCash\Key.Pub\ecash - Copy.pkcs8");

            //using (var reader = File.OpenText(@"F:\SVN\eCash\eCash\keys\ecash.key"))
            using (var reader = File.OpenText(@"c:\ECPAY\CNTT-ECPAY\eCash Project\eCash\4.Sources\2.DRAFT\eCash_nphan\Key.Pub\privateKey.pem"))
                keyPair = (AsymmetricCipherKeyPair)new Org.BouncyCastle.OpenSsl.PemReader(reader).ReadObject();


            IAsymmetricBlockCipher eng = new Pkcs1Encoding(new RsaEngine());
            eng.Init(false, keyPair.Private);
            var cryptMessage = Encoding.UTF8.GetString(eng.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));


            //var decryptEngine = new RsaEngine();
            //decryptEngine.Init(false, keyPair.Private);
            //var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));


        }


    }

    public class PasswordFinder : IPasswordFinder
    {
        private string password;

        public PasswordFinder(string password)
        {
            this.password = password;
        }


        public char[] GetPassword()
        {
            return password.ToCharArray();
        }
    }
}
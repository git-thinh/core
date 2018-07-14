//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;

//namespace core.Db
//{

//    public class dbSecurety
//    {
//        static string securityKey = "$$$$$";

//        public static void CreateFileBlank(string file, int SizeDesired_MB)
//        {
//            if (SizeDesired_MB < 1) SizeDesired_MB = 1;

//            //string file = @"e:\ifc.db";
//            //long length_add = 1024L * 1024L * 1L; // 1MB
//            long length_add = 1024L * 1024L * SizeDesired_MB; // SizeDesired_MB MB

//            // create blank file of desired size (nice and quick!)
//            FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
//            fs.Seek(length_add, SeekOrigin.Begin);
//            fs.WriteByte(0);
//            fs.Close();
//        }


//        public static string Encrypt(string toEncrypt, bool useHashing = true)
//        {

//            string retVal = string.Empty;

//            try
//            {
//                byte[] keyArray;

//                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

//                // Validate inputs 
//                // If hashing use get hashcode regards to your key

//                if (useHashing)
//                {
//                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
//                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
//                    // Always release the resources and flush data
//                    // of the Cryptographic service provide. Best Practice
//                    hashmd5.Clear();
//                }
//                else
//                {
//                    keyArray = UTF8Encoding.UTF8.GetBytes(securityKey);
//                }

//                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
//                // Set the secret key for the tripleDES algorithm
//                tdes.Key = keyArray;
//                // Mode of operation. there are other 4 modes.
//                // We choose ECB (Electronic code Book)
//                tdes.Mode = CipherMode.ECB;

//                // Padding mode (if any extra byte added)

//                tdes.Padding = PaddingMode.PKCS7;

//                ICryptoTransform cTransform = tdes.CreateEncryptor();
//                // Transform the specified region of bytes array to resultArray
//                byte[] resultArray =
//                  cTransform.TransformFinalBlock(toEncryptArray, 0,
//                  toEncryptArray.Length);

//                // Release resources held by TripleDes Encryptor
//                tdes.Clear();

//                // Return the encrypted data into unreadable string format
//                retVal = Convert.ToBase64String(resultArray, 0, resultArray.Length);
//            }
//            catch (Exception ex)
//            {
//                //throw new EncryptionException(EncryptionException.Code.EncryptionFailure, ex, MethodBase.GetCurrentMethod());
//            }
//            return retVal;
//        }

//        public static string Decrypt(string cipherString, bool useHashing = true)
//        {

//            string retVal = string.Empty;

//            try
//            {
//                byte[] keyArray;

//                byte[] toEncryptArray = Convert.FromBase64String(cipherString);

//                if (useHashing)
//                {
//                    // If hashing was used get the hash code with regards to your key

//                    MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
//                    keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(securityKey));
//                    // Release any resource held by the MD5CryptoServiceProvider
//                    hashmd5.Clear();
//                }
//                else
//                {
//                    // If hashing was not implemented get the byte code of the key
//                    keyArray = UTF8Encoding.UTF8.GetBytes(securityKey);
//                }
//                TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

//                // Set the secret key for the tripleDES algorithm
//                tdes.Key = keyArray;

//                // Mode of operation. there are other 4 modes.
//                // We choose ECB(Electronic code Book)
//                tdes.Mode = CipherMode.ECB;

//                // Padding mode(if any extra byte added)
//                tdes.Padding = PaddingMode.PKCS7;

//                ICryptoTransform cTransform = tdes.CreateDecryptor();
//                byte[] resultArray = cTransform.TransformFinalBlock(

//                                     toEncryptArray, 0, toEncryptArray.Length);
//                // Release resources held by TripleDes Encryptor
//                tdes.Clear();
//                // Return the Clear decrypted TEXT
//                retVal = UTF8Encoding.UTF8.GetString(resultArray);
//            }
//            catch (Exception ex)
//            {
//                //throw new EncryptionException(EncryptionException.Code.DecryptionFailure, ex, MethodBase.GetCurrentMethod());
//            }
//            return retVal;
//        }
//    }

//}

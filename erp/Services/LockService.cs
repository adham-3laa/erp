using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace erp.Services
{
    /// <summary>
    /// Manages the system time-lock state with secure encrypted storage.
    /// One correct password = permanent unlock.
    /// 100 failed attempts = permanent lock.
    /// </summary>
    public class LockService
    {
        private const string LOCK_FILE_NAME = ".syslock";
        private const string UNLOCK_PASSWORD = "119119";
        private const int MAX_FAILED_ATTEMPTS = 100; // عدد المحاولات الخاطئة المسموحة
        
        // 📅 تاريخ تفعيل القفل: 1 فبراير 2026
        private static readonly DateTime LOCK_DATE = new DateTime(2026, 2, 1);
        
        // Encryption key derived from machine-specific data
        private static readonly byte[] EncryptionKey = DeriveEncryptionKey();
        
        private readonly string _lockFilePath;

        public LockService()
        {
            // Store lock file in app data directory
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ERPSystem"
            );
            
            Directory.CreateDirectory(appDataPath);
            _lockFilePath = Path.Combine(appDataPath, LOCK_FILE_NAME);
        }

        /// <summary>
        /// Checks if the system should be locked based on date and unlock status.
        /// </summary>
        public bool IsSystemLocked()
        {
            // If permanently locked (100 failed attempts), always lock
            if (IsPermanentlyLocked())
            {
                return true;
            }

            // If already unlocked permanently (correct password was entered), never lock again
            if (IsUnlockedPermanently())
            {
                return false;
            }

            // Check if current date is on or after lock date
            DateTime currentDate = DateTime.Now.Date;
            return currentDate >= LOCK_DATE;
        }

        /// <summary>
        /// Validates the unlock password.
        /// </summary>
        public bool ValidatePassword(string password)
        {
            return password == UNLOCK_PASSWORD;
        }

        /// <summary>
        /// Records a successful password entry - unlocks permanently.
        /// </summary>
        public void RecordSuccessfulAttempt()
        {
            try
            {
                ErrorHandlingService.LogInfo("Correct password entered - unlocking permanently.");
                UnlockPermanently();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Failed to record successful attempt");
                throw;
            }
        }

        /// <summary>
        /// Records a failed password entry.
        /// After 100 failed attempts, locks permanently.
        /// Returns remaining attempts (0 means permanently locked).
        /// </summary>
        public int RecordFailedAttempt()
        {
            try
            {
                int currentFailed = GetFailedAttempts();
                currentFailed++;

                ErrorHandlingService.LogInfo($"Failed unlock attempt #{currentFailed} of {MAX_FAILED_ATTEMPTS}");

                if (currentFailed >= MAX_FAILED_ATTEMPTS)
                {
                    // Reached 100 failed attempts - lock permanently
                    LockPermanently();
                    ErrorHandlingService.LogInfo("System permanently locked after 100 failed attempts.");
                    return 0;
                }
                else
                {
                    // Save the new failed attempt count
                    SaveFailedAttempts(currentFailed);
                    return MAX_FAILED_ATTEMPTS - currentFailed;
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Failed to record failed attempt");
                throw;
            }
        }

        /// <summary>
        /// Gets the current number of failed attempts.
        /// </summary>
        public int GetFailedAttempts()
        {
            try
            {
                if (!File.Exists(_lockFilePath))
                {
                    return 0;
                }

                byte[] encryptedData = File.ReadAllBytes(_lockFilePath);
                string data = DecryptData(encryptedData);
                
                string[] parts = data.Split('|');
                
                // Check if it's permanent lock
                if (parts.Length >= 1 && parts[0] == "LOCKED")
                {
                    return MAX_FAILED_ATTEMPTS; // Permanently locked
                }
                
                // Check if it's permanent unlock
                if (parts.Length >= 1 && parts[0] == "UNLOCKED")
                {
                    return 0; // Unlocked, no failed attempts matter
                }
                
                // Check if it's failed attempt count
                if (parts.Length >= 2 && parts[0] == "FAILED" && int.TryParse(parts[1], out int count))
                {
                    return count;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets remaining failed attempts before permanent lock.
        /// </summary>
        public int GetRemainingAttempts()
        {
            int failed = GetFailedAttempts();
            return Math.Max(0, MAX_FAILED_ATTEMPTS - failed);
        }

        /// <summary>
        /// Checks if system is permanently locked (100 failed attempts).
        /// </summary>
        public bool IsPermanentlyLocked()
        {
            try
            {
                if (!File.Exists(_lockFilePath))
                {
                    return false;
                }

                byte[] encryptedData = File.ReadAllBytes(_lockFilePath);
                string data = DecryptData(encryptedData);
                
                return ValidateLockData(data);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves the current failed attempt count.
        /// </summary>
        private void SaveFailedAttempts(int count)
        {
            string machineId = GetMachineIdentifier();
            string data = $"FAILED|{count}|{DateTime.Now:O}|{machineId}";
            string hash = ComputeHash(data);
            string finalData = $"{data}|{hash}";
            
            byte[] encryptedData = EncryptData(finalData);
            File.WriteAllBytes(_lockFilePath, encryptedData);
        }

        /// <summary>
        /// Permanently locks the system after 100 failed attempts.
        /// </summary>
        private void LockPermanently()
        {
            try
            {
                string lockData = CreateLockData();
                byte[] encryptedData = EncryptData(lockData);
                File.WriteAllBytes(_lockFilePath, encryptedData);
                
                ErrorHandlingService.LogInfo("System permanently locked.");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Failed to write permanent lock state");
                throw;
            }
        }

        /// <summary>
        /// Creates permanent lock data.
        /// </summary>
        private string CreateLockData()
        {
            DateTime lockTime = DateTime.Now;
            string machineId = GetMachineIdentifier();
            string data = $"LOCKED|{lockTime:O}|{machineId}";
            
            string hash = ComputeHash(data);
            return $"{data}|{hash}";
        }

        /// <summary>
        /// Validates permanent lock data.
        /// </summary>
        private bool ValidateLockData(string lockData)
        {
            if (string.IsNullOrEmpty(lockData))
            {
                return false;
            }

            string[] parts = lockData.Split('|');
            
            if (parts.Length == 4 && parts[0] == "LOCKED")
            {
                string machineId = GetMachineIdentifier();
                if (parts[2] != machineId)
                {
                    return false;
                }

                string dataToHash = $"{parts[0]}|{parts[1]}|{parts[2]}";
                string expectedHash = ComputeHash(dataToHash);
                
                return parts[3] == expectedHash;
            }

            return false;
        }

        /// <summary>
        /// Permanently unlocks the system after correct password.
        /// </summary>
        private void UnlockPermanently()
        {
            try
            {
                string unlockData = CreateUnlockData();
                byte[] encryptedData = EncryptData(unlockData);
                File.WriteAllBytes(_lockFilePath, encryptedData);
                
                ErrorHandlingService.LogInfo("System permanently unlocked.");
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Failed to write unlock state");
                throw;
            }
        }

        /// <summary>
        /// Checks if the system has been permanently unlocked (correct password was entered).
        /// </summary>
        private bool IsUnlockedPermanently()
        {
            try
            {
                if (!File.Exists(_lockFilePath))
                {
                    return false;
                }

                byte[] encryptedData = File.ReadAllBytes(_lockFilePath);
                string unlockData = DecryptData(encryptedData);
                
                return ValidateUnlockData(unlockData);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates unlock data with timestamp and validation hash.
        /// </summary>
        private string CreateUnlockData()
        {
            DateTime unlockTime = DateTime.Now;
            string machineId = GetMachineIdentifier();
            string data = $"UNLOCKED|{unlockTime:O}|{machineId}";
            
            string hash = ComputeHash(data);
            return $"{data}|{hash}";
        }

        /// <summary>
        /// Validates the unlock data structure and hash.
        /// </summary>
        private bool ValidateUnlockData(string unlockData)
        {
            if (string.IsNullOrEmpty(unlockData))
            {
                return false;
            }

            string[] parts = unlockData.Split('|');
            
            if (parts.Length == 4 && parts[0] == "UNLOCKED")
            {
                string machineId = GetMachineIdentifier();
                if (parts[2] != machineId)
                {
                    return false;
                }

                string dataToHash = $"{parts[0]}|{parts[1]}|{parts[2]}";
                string expectedHash = ComputeHash(dataToHash);
                
                return parts[3] == expectedHash;
            }

            return false;
        }

        /// <summary>
        /// Derives encryption key from machine-specific data.
        /// </summary>
        private static byte[] DeriveEncryptionKey()
        {
            string seed = $"{Environment.MachineName}|{Environment.OSVersion}|ERPSystemLock2026";
            
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
            }
        }

        /// <summary>
        /// Gets a machine-specific identifier.
        /// </summary>
        private string GetMachineIdentifier()
        {
            string identifier = $"{Environment.MachineName}|{Environment.UserName}";
            return ComputeHash(identifier);
        }

        /// <summary>
        /// Computes SHA256 hash of the input string.
        /// </summary>
        private string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Encrypts data using AES encryption.
        /// </summary>
        private byte[] EncryptData(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts data using AES encryption.
        /// </summary>
        private string DecryptData(byte[] cipherData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;

                byte[] iv = new byte[aes.IV.Length];
                Array.Copy(cipherData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(cipherData, iv.Length, cipherData.Length - iv.Length))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}

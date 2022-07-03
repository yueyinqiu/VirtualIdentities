using System.Security.Cryptography;
using System.Text.Json;

namespace VirtualIdentities.Core
{
    public sealed class Identity : IDisposable
    {
        private readonly RSA algorithm;
        public string Name { get; }

        /// <exception cref="ArgumentNullException"></exception>
        public Identity(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            this.Name = name;
            this.algorithm = RSA.Create();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public Identity(string name,
            byte[] encryptedKey,
            string password) : this(name)
        {
            if (encryptedKey is null)
                throw new ArgumentNullException(nameof(encryptedKey));

            if (password is null)
                throw new ArgumentNullException(nameof(password));

            try
            {
                this.algorithm.ImportEncryptedPkcs8PrivateKey(password, encryptedKey, out _);
            }
            catch (CryptographicException e)
            {
                throw new CryptographicException(
                    $"{nameof(password)}({password}) is wrong or {nameof(encryptedKey)} is invalid.", e);
            }
        }

        public Card CreateCard(string? name = null)
        {
            return new Card(name ?? this.Name, algorithm.ExportRSAPublicKey());
        }

        private class SerializationModel
        {
            public string? Name { get; set; }
            public byte[]? Key { get; set; }
        }

        private readonly static PbeParameters pbeParameters = new PbeParameters(
            PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, iterationCount: 4096);

        /// <exception cref="ArgumentNullException"></exception>
        public string ExportAsString(string password)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            var model = new SerializationModel
            {
                Key = algorithm.ExportEncryptedPkcs8PrivateKey(password, pbeParameters),
                Name = this.Name
            };
            return JsonSerializer.Serialize(model);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static Identity? FromExportedString(string stringExported, string password)
        {
            if (stringExported is null)
                throw new ArgumentNullException(nameof(stringExported));

            if (password is null)
                throw new ArgumentNullException(nameof(password));

            SerializationModel? model;
            try
            {
                model = JsonSerializer.Deserialize<SerializationModel>(stringExported);
            }
            catch (JsonException e)
            {
                throw new FormatException($"Cannot parse to {nameof(Identity)}.", e);
            }
            if (model is null)
                return null;
            if (model.Name is null || model.Key is null)
                throw new FormatException($"Cannot parse to {nameof(Identity)}.");
            return new Identity(model.Name, model.Key, password);
        }

        public void Dispose()
        {
            this.algorithm.Dispose();
        }
    }
}
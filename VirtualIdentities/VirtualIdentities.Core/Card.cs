using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VirtualIdentities.Core.Extensions;

namespace VirtualIdentities.Core
{
    public sealed class Card : IDisposable
    {
        private readonly RSA algorithm;
        public void Dispose()
        {
            algorithm.Dispose();
        }

        public string Name { get; }

        public byte[] Key => algorithm.ExportRSAPublicKey();

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CryptographicException"></exception>
        internal Card(string name, byte[] key)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (key is null)
                throw new ArgumentNullException(nameof(key));

            this.Name = name;
            this.algorithm = RSA.Create();
            try
            {
                this.algorithm.ImportRSAPublicKey(key, out _);
            }
            catch(CryptographicException e)
            {
                throw new CryptographicException($"{key} is invalid.", e);
            }
        }

        private class SerializationModel
        {
            public string? Name { get; set; }
            public byte[]? Key { get; set; }
        }

        public string ExportAsString()
        {
            var model = new SerializationModel
            {
                Key = algorithm.ExportRSAPublicKey(),
                Name = this.Name
            };
            return JsonSerializer.Serialize(model);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="CryptographicException"></exception>
        public static Card? FromExportedString(string stringExported)
        {
            if (stringExported is null)
                throw new ArgumentNullException(nameof(stringExported));

            SerializationModel? model;
            try
            {
                model = JsonSerializer.Deserialize<SerializationModel>(stringExported);
            }
            catch (JsonException e)
            {
                throw new FormatException($"Cannot parse to {nameof(Card)}.", e);
            }
            if (model is null)
                return null;
            if (model.Name is null || model.Key is null)
                throw new FormatException($"Cannot parse to {nameof(Card)}.");
            return new Card(model.Name, model.Key);
        }
    }
}

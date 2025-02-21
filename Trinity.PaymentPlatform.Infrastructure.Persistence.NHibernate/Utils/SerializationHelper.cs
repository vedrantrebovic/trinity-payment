﻿using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Trinity.PaymentPlatform.Infrastructure.Persistence.NHibernate.Utils
{

    public static class SerializationHelper
    {
        /// <summary>
        /// Internal utility method to convert an UTF8 Byte Array to an UTF8 string.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        private static string UTF8ByteArrayToString(byte[] characters)
        {
            return new UTF8Encoding().GetString(characters);
        }

        /// <summary>
        /// Internal utility method to convert an UTF8 string to an UTF8 Byte Array.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static byte[] StringToUTF8ByteArray(string xml)
        {
            return new UTF8Encoding().GetBytes(xml);
        }

        /// <summary>
        /// Serialize a [Serializable] object of type T into an XML/UTF8 string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(this T item)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlTextWriter xml = new XmlTextWriter(stream, new UTF8Encoding(false)))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(T));
                        xs.Serialize(xml, item);
                        return UTF8ByteArrayToString(((MemoryStream)xml.BaseStream).ToArray());
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// De-serialize an XML/UTF8 string into an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string xml)
        {
            using (MemoryStream stream = new MemoryStream(StringToUTF8ByteArray(xml)))
            using (new XmlTextWriter(stream, new UTF8Encoding(false)))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
            }
        }

        /// <summary>
        /// Serialize an object T using BinaryFormatter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        [Obsolete("Obsolete")]
        public static string SerializeObjectUsingBinaryFormatter<T>(this T item)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, item);
                return UTF8ByteArrayToString(ms.ToArray());
            }
        }

        /// <summary>
        /// De-serialize a BinaryFormatter-serialized string into an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        [Obsolete("Obsolete")]
        public static T DeserializeObjectUsingBinaryFormatter<T>(string str)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(StringToUTF8ByteArray(str)))
            {
                return (T)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// Serialize a [Serializable] object of type T into an XML-formatted string using XmlSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">any T object</param>
        /// <returns>an XML-formatted string</returns>
        public static string SerializeToXML<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringWriter = new StringWriter();
                var encoding = Encoding.GetEncoding("ISO-8859-1");
                XmlWriterSettings settings = new XmlWriterSettings()
                {
                    Indent = true, OmitXmlDeclaration = false, Encoding = Encoding.GetEncoding("ISO-8859-1")
                };

                using (var stream = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(stream, settings))
                    {
                        xmlserializer.Serialize(writer, value);
                        return encoding.GetString(stream.ToArray());
                    }
                }
                using (var writer = XmlWriter.Create(stringWriter, settings))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        /// <summary>
        /// De-serialize a [Serializable] object of type T into an XML-formatted string using XmlSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">any T object</param>
        /// <returns>an XML-formatted string</returns>
        public static T DeserializeFromXML<T>(string xml)
        {
            if (String.IsNullOrEmpty(xml)) throw new NotSupportedException("ERROR: input string cannot be null.");
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                var stringReader = new StringReader(xml);
                
                using (var reader = XmlReader.Create(stringReader))
                {
                    return (T)xmlserializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }
    }
}

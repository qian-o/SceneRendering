using System.Text;

namespace Core.Helpers;

public static unsafe class StringExtensions
{
    private static readonly Dictionary<EncodingType, Encoding> _encodings = new();

    public static string ToString(this byte[] buffer, EncodingType encodingType = EncodingType.UTF8, int? length = null)
    {
        Encoding encoding = GetEncoding(encodingType);

        if (length.HasValue)
        {
            return encoding.GetString(buffer, 0, length.Value).TrimEnd('\0');
        }
        else
        {
            return encoding.GetString(buffer).TrimEnd('\0');
        }
    }

    private static Encoding GetEncoding(EncodingType encodingType)
    {
        if (!_encodings.TryGetValue(encodingType, out Encoding? encoding))
        {
            encoding = encodingType switch
            {
                EncodingType.UTF8 => Encoding.UTF8,
                EncodingType.UTF32 => Encoding.UTF32,
                EncodingType.ASCII => Encoding.ASCII,
                EncodingType.BigEndianUnicode => Encoding.BigEndianUnicode,
                EncodingType.Default => Encoding.Default,
                EncodingType.Unicode => Encoding.Unicode,
                EncodingType.ShiftJIS => CodePagesEncodingProvider.Instance.GetEncoding("Shift-JIS")!,
                _ => throw new NotImplementedException()
            };

            _encodings.Add(encodingType, encoding);
        }

        return encoding;
    }
}

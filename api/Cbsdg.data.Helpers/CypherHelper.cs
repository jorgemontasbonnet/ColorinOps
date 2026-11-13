using System.Text;

namespace Cbsdg.Data.Helpers;

public class CypherHelper()
{
    private const ushort LicenseKey = 1974;
    private const ushort c1 = 52845;
    private const ushort c2 = 22719;

    public static string DelphiDecodeString(string inStr)
    {
        /*class function TLicenseManager.DecodeString(const inStr: string): string;
var
 tmpStr: string;
 hexStr: string;
 j: Integer;
begin
  j := 1;
  while j < Length(inStr) do begin
    hexStr := Copy(inStr,j,2);
    tmpStr := tmpStr + Chr(StrToInt('0x' + hexStr));
    Inc(j,2);
  end;
  Result := tmpStr;
end;*/
      var j = 0;
        var tmpStr = string.Empty;
        while ((j + 2) <= inStr.Length)
        {
            var hexStr = inStr.Substring(j, 2);
            tmpStr += (char)Convert.ToInt32(hexStr, 16);
            j += 2;
        }
        return tmpStr;
    }
    public static string DecodeString(string inStr)
    {
        try
        {
            var bytes = Convert.FromBase64String(inStr);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string EncodeString(string inStr)
    {
        var bytes = Encoding.UTF8.GetBytes(inStr);
        return Convert.ToBase64String(bytes);
    }

    public static string DeEncryptString(string inStr) => DeEncryptString(inStr, LicenseKey);
        
    public static string DeEncryptString(string inStr, ushort key)
    {
        var decodedStr = DelphiDecodeString(inStr);
        var resultStr = new char[decodedStr.Length];
        for (var i = 0; i < decodedStr.Length; i++)
        {
            resultStr[i] = (char)(decodedStr[i] ^ key >> 8);
            byte c = (byte) decodedStr[i];
            key = (ushort) ((c + key) * c1 + c2);
        }
        return new string(resultStr);
    }

    public static string EncryptString(string inStr) => EncryptString(inStr, LicenseKey);
    public static string EncryptString(string inStr, ushort key)
    {
        var resultStr = new char[inStr.Length];
        for (var i = 0; i < inStr.Length; i++)
        {
            resultStr[i] = (char)(inStr[i] ^ key >> 8);
            key = (byte) ((resultStr[i] + key) * c1 + c2);
        }
        return EncodeString(new string(resultStr));
    }



    public static string SaltEncrypt(string inStr, string salt)
    {
        var resultStr = string.Empty;
        if (string.IsNullOrEmpty(salt))
        {
            return inStr;
        }
        var startKey = salt.Length;
        var multKey = (byte)salt[0];
        byte addKey = 0;
        for (var i = 0; i < salt.Length - 1; i++)
        {
            addKey += (byte)salt[i];
        }
        for (var i = 0; i < inStr.Length; i++)
        {
            resultStr += (char)((byte)inStr[i] ^ startKey >> 8);
            startKey = ((byte)resultStr[i] + startKey) * multKey + addKey;
        }
        return resultStr;
    }


    public static string SaltDecrypt(string inStr, string salt)
    {
        var resultStr = string.Empty;
        if (string.IsNullOrEmpty(salt))
        {
            return inStr;
        }
        var startKey = (byte)salt[0];
        byte multKey = (byte) salt.Length;
        byte addKey = 0;
        for (var i = 0; i < salt.Length - 1; i++)
        {
            addKey += (byte)salt[i];
        }
        for (var i = 0; i < inStr.Length; i++)
        {
            resultStr += (char)((byte)inStr[i] ^ startKey >> 8);
            startKey = (byte)(((byte)inStr[i] + startKey) * multKey + addKey);
        }

        return resultStr; 
    }
    
    public static bool PasswordsMatch(string plainPwd, string hashedPwd)
    {
        var decryptedPwd = DeEncryptString(hashedPwd);
        var parts = decryptedPwd.Split('.');
        if (parts.Length != 2) return false;
        var salt = DecodeString(parts[0]);
        var hash = DecodeString(parts[1]);

        var hashedInputPwd = SaltEncrypt(plainPwd, salt);
        return hashedInputPwd == hash;
    }    
}

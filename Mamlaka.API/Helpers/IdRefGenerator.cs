namespace Mamlaka.API.Helpers;

public class IdRefGenerator
{
    public string KeyGenerator(string _prefix, int _keyLength)
    {
        string characterChoice = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        string characters = characterChoice;
        string _number = string.Empty;
        for (int i = 0; i < _keyLength; i++)
        {
            string character;
            do
            {
                int index = new Random().Next(0, characters.Length);
                character = characters.ToCharArray()[index].ToString();
            } while (_number.IndexOf(character) != -1);
            _number += character;
        }
        return $"{_prefix}{_number}";
    }
   
}

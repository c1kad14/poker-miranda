using System;

namespace miranda.JabberBot
{
    public static class Common
    {
        public static DateTime Now
        {
            get { return DateTime.Now; }
        }


        static char[] eng = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '[', ']',
                            'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';',  
                            'z', 'x', 'c', 'v', 'b', 'n', 'm'};

        static char[] rus = { 'й', 'ц', 'у', 'к', 'е', 'н', 'г', 'ш', 'щ', 'з', 'х', 'ъ',
                            'ф', 'ы', 'в', 'а', 'п', 'р', 'о', 'л', 'д', 'ж',  
                            'я', 'ч', 'с', 'м', 'и', 'т', 'ь'};

        static char CharReplacer(char ch)
        {
            int index = 0;
            for (int i = 0; i < rus.Length; i++)
            {
                if (rus[i] == ch)
                {
                    index = i;
                    break;
                }
            }
            return eng[index];
        }

        public static string TransCode(string msg)
        {
            string result = string.Empty;
            for (int i = 0; i < msg.Length; i++)
            {
                result += CharReplacer(msg[i]);
            }
            return result;
        }
    }
}
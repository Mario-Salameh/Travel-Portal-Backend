public static class CurrencyToWordsService
{
    public static string ToWordsEnglish(double amount)
    {
        if (double.IsNaN(amount)) return string.Empty;
        if (amount == 0) return "Zero Dollars";

        var isNegative = amount < 0;
        amount = Math.Abs(amount);

        var ones = new[] { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        var teens = new[] { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        var tens = new[] { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        var thousands = new[] { "", "thousand", "million", "billion", "trillion" };

        string ConvertChunk(int n)
        {
            if (n == 0) return string.Empty;
            string words = string.Empty;

            if (n >= 100)
            {
                words += ones[n / 100] + " hundred ";
                n %= 100;
            }

            if (n >= 20)
            {
                words += tens[n / 10] + " ";
                n %= 10;
            }

            if (n >= 10)
            {
                return words + teens[n - 10] + " ";
            }

            if (n > 0)
            {
                words += ones[n] + " ";
            }

            return words;
        }

        int integerPart = (int)Math.Floor(amount);
        int decimalPart = (int)Math.Round((amount - integerPart) * 100);

        string integerWords;
        if (integerPart == 0)
        {
            integerWords = "Zero";
        }
        else
        {
            integerWords = string.Empty;
            int tempNum = integerPart;
            int i = 0;

            do
            {
                int chunk = tempNum % 1000;
                if (chunk != 0)
                {
                    integerWords = ConvertChunk(chunk) + thousands[i] + " " + integerWords;
                }
                tempNum = tempNum / 1000;
                i++;
            } while (tempNum > 0);
        }

        string result = System.Text.RegularExpressions.Regex.Replace(integerWords.Trim(), @"\s+", " ");
        result = char.ToUpper(result[0]) + result.Substring(1);
        result += " Dollars";

        if (decimalPart > 0)
        {
            string decimalWords = ConvertChunk(decimalPart).Trim();
            decimalWords = System.Text.RegularExpressions.Regex.Replace(decimalWords, @"\s+", " ");
            decimalWords = char.ToUpper(decimalWords[0]) + decimalWords.Substring(1);
            result += $" and {decimalWords} Cents";
        }

        if (isNegative)
            result = $"Negative {result}";

        return result;
    }

    public static string ToWordsArabic(double amount)
    {
        if (double.IsNaN(amount)) return string.Empty;

        var isNegative = amount < 0;
        amount = Math.Abs(amount);

        int integerPart = (int)Math.Floor(amount);
        int decimalPart = (int)Math.Round((amount - integerPart) * 100);

        var ones = new[] { "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة" };
        var teens = new[] { "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر", "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر" };
        var tens = new[] { "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون" };
        var hundreds = new[] { "", "مائة", "مئتان", "ثلاثمائة", "أربعمائة", "خمسمائة", "ستمائة", "سبعمائة", "ثمانمائة", "تسعمائة" };
        var thousands = new[] { "", "ألف", "ألفان", "آلاف", "مليون", "مليونان", "ملايين", "مليار", "ملياران", "مليارات" };

        string Convert(int n)
        {
            if (n == 0) return string.Empty;
            if (n < 10) return ones[n];
            if (n < 20) return teens[n - 10];

            if (n < 100)
            {
                int one = n % 10;
                int ten = n / 10;
                return ones[one] + (one > 0 ? " و" : "") + tens[ten];
            }

            if (n < 1000)
            {
                int hundred = n / 100;
                int rest = n % 100;
                return hundreds[hundred] + (rest > 0 ? " و" : "") + Convert(rest);
            }

            if (n < 1000000)
            {
                int thousand = n / 1000;
                int rest = n % 1000;
                string thousandWord;

                if (thousand == 1)
                    thousandWord = thousands[1];
                else if (thousand == 2)
                    thousandWord = thousands[2];
                else if (thousand >= 3 && thousand <= 10)
                    thousandWord = thousands[3];
                else
                    thousandWord = thousands[1];

                if (thousand >= 3 && thousand <= 10)
                {
                    return Convert(thousand).Replace("واحد", "ألف").Replace("اثنان", "ألفان") + " " + thousands[3] + (rest > 0 ? " و" : "") + Convert(rest);
                }

                return Convert(thousand) + " " + thousandWord + (rest > 0 ? " و" : "") + Convert(rest);
            }

            return string.Empty;
        }

        string integerWords = integerPart == 0 ? "صفر" : Convert(integerPart);
        string decimalWords = decimalPart > 0 ? Convert(decimalPart) : string.Empty;

        string result = $"{integerWords} دولار أمريكي";

        if (!string.IsNullOrEmpty(decimalWords))
        {
            result += $" و {decimalWords} سنت";
        }

        result += " فقط";

        if (isNegative)
            result = $"سالب {result}";

        return result.Trim();
    }
}
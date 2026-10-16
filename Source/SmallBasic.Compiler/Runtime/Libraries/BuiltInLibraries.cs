using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmallBasic.Compiler.Runtime
{
    public class BuiltInLibraries : IEngineLibraries
    {
        public BuiltInLibraries()
        {
            this.Math = new BuiltInMathLibrary();
            this.Text = new BuiltInTextLibrary();
            this.Array = new BuiltInArrayLibrary();
        }

        public IArrayLibrary Array { get; }
        public IClockLibrary Clock => null;
        public IControlsLibrary Controls => null;
        public IDesktopLibrary Desktop => null;
        public IDictionaryLibrary Dictionary => null;
        public IFileLibrary File => null;
        public IFlickrLibrary Flickr => null;
        public IGraphicsWindowLibrary GraphicsWindow => null;
        public IImageListLibrary ImageList => null;
        public IMathLibrary Math { get; }
        public IMouseLibrary Mouse => null;
        public INetworkLibrary Network => null;
        public IProgramLibrary Program => null;
        public IShapesLibrary Shapes => null;
        public ISoundLibrary Sound => null;
        public IStackLibrary Stack => null;
        public ITextLibrary Text { get; }
        public ITextWindowLibrary TextWindow => null;
        public ITimerLibrary Timer => null;
        public ITurtleLibrary Turtle => null;

        private class BuiltInMathLibrary : IMathLibrary
        {
            public decimal Get_Pi() => (decimal)System.Math.PI;
            public decimal Abs(decimal number) => System.Math.Abs(number);
            public decimal Ceiling(decimal number) => System.Math.Ceiling(number);
            public decimal Floor(decimal number) => System.Math.Floor(number);
            public decimal NaturalLog(decimal number) => (decimal)System.Math.Log((double)number);
            public decimal Log(decimal number) => (decimal)System.Math.Log10((double)number);
            public decimal Cos(decimal angle) => (decimal)System.Math.Cos((double)angle);
            public decimal Sin(decimal angle) => (decimal)System.Math.Sin((double)angle);
            public decimal Tan(decimal angle) => (decimal)System.Math.Tan((double)angle);
            public decimal ArcCos(decimal cosValue) => (decimal)System.Math.Acos((double)cosValue);
            public decimal ArcSin(decimal sinValue) => (decimal)System.Math.Asin((double)sinValue);
            public decimal ArcTan(decimal tanValue) => (decimal)System.Math.Atan((double)tanValue);
            public decimal GetDegrees(decimal angle) => angle * (180m / (decimal)System.Math.PI);
            public decimal GetRadians(decimal angle) => angle * ((decimal)System.Math.PI / 180m);
            public decimal SquareRoot(decimal number) => (decimal)System.Math.Sqrt((double)number);
            public decimal Power(decimal baseNumber, decimal exponent) => (decimal)System.Math.Pow((double)baseNumber, (double)exponent);
            public decimal Round(decimal number) => System.Math.Round(number);
            public decimal Max(decimal number1, decimal number2) => System.Math.Max(number1, number2);
            public decimal Min(decimal number1, decimal number2) => System.Math.Min(number1, number2);
            public decimal Remainder(decimal dividend, decimal divisor) => divisor == 0 ? 0 : dividend % divisor;
            public decimal GetRandomNumber(decimal maxNumber)
            {
                int max = (int)maxNumber;
                return max <= 0 ? 0 : new Random().Next(1, max + 1);
            }
        }

        private class BuiltInTextLibrary : ITextLibrary
        {
            public string Append(string text1, string text2) => (text1 ?? "") + (text2 ?? "");
            public decimal GetLength(string text) => (text ?? "").Length;
            public string GetSubText(string text, decimal start, decimal length)
            {
                if (string.IsNullOrEmpty(text) || start < 1 || length <= 0) return "";
                int startIndex = (int)start - 1;
                int len = (int)length;
                if (startIndex >= text.Length) return "";
                if (startIndex + len > text.Length) len = text.Length - startIndex;
                return text.Substring(startIndex, len);
            }

            public string GetSubTextToEnd(string text, decimal start)
            {
                if (string.IsNullOrEmpty(text) || start < 1) return "";
                int startIndex = (int)start - 1;
                if (startIndex >= text.Length) return "";
                return text.Substring(startIndex);
            }

            public decimal GetIndexOf(string text, string subText)
            {
                if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(subText)) return 0;
                int idx = text.IndexOf(subText, StringComparison.Ordinal);
                return idx < 0 ? 0 : idx + 1;
            }

            public string ConvertToLowerCase(string text) => (text ?? "").ToLowerInvariant();
            public string ConvertToUpperCase(string text) => (text ?? "").ToUpperInvariant();
            public string GetCharacter(decimal characterCode) => ((char)(int)characterCode).ToString();
            public decimal GetCharacterCode(string character) => string.IsNullOrEmpty(character) ? 0 : (decimal)character[0];
            public bool IsSubText(string text, string subText) => (text ?? "").Contains(subText ?? "");
            public bool StartsWith(string text, string subText) => (text ?? "").StartsWith(subText ?? "", StringComparison.Ordinal);
            public bool EndsWith(string text, string subText) => (text ?? "").EndsWith(subText ?? "", StringComparison.Ordinal);
        }

        private class BuiltInArrayLibrary : IArrayLibrary
        {
            public bool ContainsIndex(ArrayValue array, string index) => !array.IsDefault() && array.ContainsKey(index);
            public bool ContainsValue(ArrayValue array, string value) => !array.IsDefault() && array.Values.Any(v => v.ToString() == value);
            public ArrayValue GetAllIndices(ArrayValue array)
            {
                if (array.IsDefault()) return new ArrayValue();
                int i = 1;
                return new ArrayValue(array.Keys.Select(k => StringValue.Create(k)).ToDictionary(value => (i++).ToString(System.Globalization.CultureInfo.CurrentCulture)));
            }
            public decimal GetItemCount(ArrayValue array) => array.IsDefault() ? 0 : array.Count;
            public bool IsArray(BaseValue array) => array is ArrayValue;
            public BaseValue GetValue(string arrayName, string index) => StringValue.Empty;
            public void SetValue(string arrayName, string index, BaseValue value) { }
            public void RemoveValue(string arrayName, string index) { }
        }
    }
}

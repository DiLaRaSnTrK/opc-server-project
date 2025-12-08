using Xunit;
using Core.Models;
using Core.Helpers;

namespace Tests
{
    public class TagValueParserTests
    {
        [Theory]
        [InlineData(TagDataType.Bool, 1, 1)]      // 1 gelirse 1 dönmeli (int olarak saklıyoruz)
        [InlineData(TagDataType.Bool, 0, 0)]      // 0 gelirse 0 dönmeli
        [InlineData(TagDataType.Bool, 55, 0)]     // Bool mantığına göre 1 dışındakiler 0 mı olmalı? (Mevcut kodunda böyle)

        [InlineData(TagDataType.Int16, 123.45, (short)123)] // Küsurat atılmalı
        [InlineData(TagDataType.Float, 12.5, 12.5f)]        // Float korunmalı
        public void Convert_ShouldReturnCorrectTypeAndValue(TagDataType type, double input, object expected)
        {
            // Act
            var result = TagValueParser.Convert(type, input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
// <copyright file="TagValueParserTests.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Tests
{
    using Core.Helpers;
    using Core.Models;
    using Xunit;

    /// <summary>
    /// TagValueParser.Convert() metodunun tüm veri tipleri için
    /// doğru tür ve değer döndürdüğünü doğrular.
    /// </summary>
    public class TagValueParserTests
    {
        // ── BOOL ─────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_Bool_One_ShouldReturn_Int1()
        {
            var result = TagValueParser.Convert(TagDataType.Bool, 1.0);
            Assert.IsType<int>(result);
            Assert.Equal(1, (int)result);
        }

        [Fact]
        public void Convert_Bool_Zero_ShouldReturn_Int0()
        {
            var result = TagValueParser.Convert(TagDataType.Bool, 0.0);
            Assert.IsType<int>(result);
            Assert.Equal(0, (int)result);
        }

        [Fact]
        public void Convert_Bool_OtherValue_ShouldReturn_Int0()
        {
            // 1 dışındaki değer → false(0)
            var result = TagValueParser.Convert(TagDataType.Bool, 55.0);
            Assert.IsType<int>(result);
            Assert.Equal(0, (int)result);
        }

        [Fact]
        public void Convert_Bool_NearOne_ShouldReturn_Int1()
        {
            // 1.0'a 0.000001 uzaklıkta → eşik içinde → true(1)
            var result = TagValueParser.Convert(TagDataType.Bool, 1.000001);
            Assert.IsType<int>(result);
            Assert.Equal(1, (int)result);
        }

        [Fact]
        public void Convert_Bool_0999_ShouldReturn_Int0()
        {
            // 0.9999 → eşik dışında → false(0)
            var result = TagValueParser.Convert(TagDataType.Bool, 0.9999);
            Assert.IsType<int>(result);
            Assert.Equal(0, (int)result);
        }

        // ── INT16 ─────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_Int16_ShouldBe_Short()
        {
            var result = TagValueParser.Convert(TagDataType.Int16, 42.0);
            Assert.IsType<short>(result);
        }

        [Theory]
        [InlineData(0.0, 0)]
        [InlineData(123.45, 123)]    // ondalık kısım atılır
        [InlineData(-50.9, -50)]     // negatif, ondalık atılır
        [InlineData(32767.0, 32767)]  // short.MaxValue
        [InlineData(-32768.0, -32768)] // short.MinValue
        public void Convert_Int16_ShouldReturn_Correct_Value(double input, int expected)
        {
            var result = TagValueParser.Convert(TagDataType.Int16, input);
            Assert.Equal((short)expected, (short)result);
        }

        // ── UINT16 ────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_UInt16_ShouldBe_UShort()
        {
            var result = TagValueParser.Convert(TagDataType.UInt16, 42.0);
            Assert.IsType<ushort>(result);
        }

        [Theory]
        [InlineData(0.0, 0)]
        [InlineData(100.7, 100)]    // ondalık kısım atılır
        [InlineData(65535.0, 65535)]  // ushort.MaxValue
        public void Convert_UInt16_ShouldReturn_Correct_Value(double input, int expected)
        {
            var result = TagValueParser.Convert(TagDataType.UInt16, input);
            Assert.Equal((ushort)expected, (ushort)result);
        }

        // ── INT32 ─────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_Int32_ShouldBe_Int()
        {
            var result = TagValueParser.Convert(TagDataType.Int32, 42.0);
            Assert.IsType<int>(result);
        }

        [Theory]
        [InlineData(0.0, 0)]
        [InlineData(100000.9, 100000)]
        [InlineData(-999999.1, -999999)]
        public void Convert_Int32_ShouldReturn_Correct_Value(double input, int expected)
        {
            var result = TagValueParser.Convert(TagDataType.Int32, input);
            Assert.Equal(expected, (int)result);
        }

        // ── UINT32 ────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_UInt32_ShouldBe_UInt()
        {
            var result = TagValueParser.Convert(TagDataType.UInt32, 42.0);
            Assert.IsType<uint>(result);
        }

        [Theory]
        [InlineData(0.0, 0u)]
        [InlineData(500000.5, 500000u)]
        public void Convert_UInt32_ShouldReturn_Correct_Value(double input, uint expected)
        {
            var result = TagValueParser.Convert(TagDataType.UInt32, input);
            Assert.Equal(expected, (uint)result);
        }

        // ── FLOAT ─────────────────────────────────────────────────────────────

        [Fact]
        public void Convert_Float_ShouldBe_Float()
        {
            var result = TagValueParser.Convert(TagDataType.Float, 42.0);
            Assert.IsType<float>(result);
        }

        [Theory]
        [InlineData(0.0, 0.0f)]
        [InlineData(12.5, 12.5f)]
        public void Convert_Float_ShouldReturn_Correct_Value(double input, float expected)
        {
            var result = TagValueParser.Convert(TagDataType.Float, input);
            // float karşılaştırmasında önce türe dönüştür
            Assert.Equal(expected, (float)result, precision: 3);
        }

        [Fact]
        public void Convert_Float_Negative_ShouldReturn_Correct_Value()
        {
            var result = TagValueParser.Convert(TagDataType.Float, -3.14);
            Assert.IsType<float>(result);
            Assert.Equal(-3.14f, (float)result, precision: 2);
        }

        // ── DOUBLE ───────────────────────────────────────────────────────────

        [Fact]
        public void Convert_Double_ShouldBe_Double()
        {
            var result = TagValueParser.Convert(TagDataType.Double, 42.0);
            Assert.IsType<double>(result);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(3.141592653589793)]
        [InlineData(-99999.12345)]
        public void Convert_Double_ShouldReturn_Same_Value(double input)
        {
            var result = TagValueParser.Convert(TagDataType.Double, input);
            Assert.Equal(input, (double)result);
        }
    }
}

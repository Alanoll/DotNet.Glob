﻿using DotNet.Globbing;
using Xunit;

namespace DotNet.Glob.Tests
{
    public class GlobTests
    {
       
        [Theory]
        [InlineData("literal", "fliteral", "foo/literal", "literals", "literals/foo")]
        [InlineData("path/hats*nd", "path/hatsblahn", "path/hatsblahndt")]
        [InlineData("path/?atstand", "path/moatstand", "path/batstands")]
        [InlineData("/**/file.csv", "/file.txt")]
        [InlineData("/*file.txt", "/folder")]
        [InlineData("Shock* 12", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("*Shock* 12", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("*ave*2", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("*ave 12", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("*ave 12", "wave 12/")]
        [InlineData("C:\\THIS_IS_A_DIR\\**\\somefile.txt", "C:\\THIS_IS_A_DIR\\awesomefile.txt")] // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/27
        [InlineData("C:\\name\\**", "C:\\name.ext", "C:\\name_longer.ext")] // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/29
        public void Does_Not_Match(string pattern, params string[] testStrings)
        {
            var glob = Globbing.Glob.Parse(pattern);
            foreach (var testString in testStrings)
            {
                Assert.False(glob.IsMatch(testString));
            }
        }
    
        [Theory]
        [InlineData("literal", "literal")]
        [InlineData("a/literal", "a/literal")]
        [InlineData("path/*atstand", "path/fooatstand")]
        [InlineData("path/hats*nd", "path/hatsforstand")]
        [InlineData("path/?atstand", "path/hatstand")]
        [InlineData("path/?atstand?", "path/hatstands")]
        [InlineData("p?th/*a[bcd]", "pAth/fooooac")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4]", "pAth/fooooacbfa2")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz]", "pAth/fooooacbfa2v")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz][!a-c][!1-3].*", "pAth/fooooacbfa2vd4.txt")]
        [InlineData("path/**/somefile.txt", "path/foo/bar/baz/somefile.txt")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz][!a-c][!1-3].*", "pGth/yGKNY6acbea3rm8.")]
        [InlineData("/**/file.*", "/folder/file.csv")]
        [InlineData("/**/file.*", "/file.txt")]
        [InlineData("/**/file.*", "/file.txt")]
        [InlineData("**/file.*", "/file.txt")]
        [InlineData("/*file.txt", "/file.txt")]
        [InlineData("C:\\THIS_IS_A_DIR\\*", "C:\\THIS_IS_A_DIR\\somefile")] // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/20
        [InlineData("/DIR1/*/*", "/DIR1/DIR2/file.txt")]  // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/21
        [InlineData("~/*~3", "~/abc123~3")]  // Regression Test for https://github.com/dazinator/DotNet.Glob/pull/15
        [InlineData("**\\Shock* 12", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("**\\*ave*2", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]       
        [InlineData("**", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12")]
        [InlineData("**", "HKEY_LOCAL_MACHINE\\SOFTWARE\\Adobe\\Shockwave 12.txt")]
        [InlineData("Stuff, *", "Stuff, x")]      // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/31
        [InlineData("\"Stuff*", "\"Stuff")]      // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/32
        public void IsMatch(string pattern, params string[] testStrings)
        {
            DotNet.Globbing.Glob.Parse("\"Stuff*", new GlobParseOptions() { AllowInvalidPathCharacters = true }).IsMatch("\"Stuff"); // true; 

        GlobParseOptions.Default.AllowInvalidPathCharacters = true;
            var glob = Globbing.Glob.Parse(pattern);
            foreach (var testString in testStrings)
            {
                var match = glob.IsMatch(testString);
                Assert.True(match);                
                           
            }
        }

        [Theory]
        [InlineData("literal", "literal")]
        [InlineData("a/literal", "a/literal")]
        [InlineData("path/*atstand", "path/fooatstand")]
        [InlineData("path/hats*nd", "path/hatsforstand")]
        [InlineData("path/?atstand", "path/hatstand")]
        [InlineData("path/?atstand?", "path/hatstands")]
        [InlineData("p?th/*a[bcd]", "pAth/fooooac")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4]", "pAth/fooooacbfa2")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz]", "pAth/fooooacbfa2v")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz][!a-c][!1-3].*", "pAth/fooooacbfa2vd4.txt")]
        [InlineData("path/**/somefile.txt", "path/foo/bar/baz/somefile.txt")]
        [InlineData("p?th/*a[bcd]b[e-g]a[1-4][!wxyz][!a-c][!1-3].*", "pGth/yGKNY6acbea3rm8.")]
        [InlineData("/**/file.*", "/folder/file.csv")]
        [InlineData("/**/file.*", "/file.txt")]
        [InlineData("/**/file.*", "/file.txt")]
        [InlineData("**/file.*", "/file.txt")]
        [InlineData("/*file.txt", "/file.txt")]
        [InlineData("C:\\THIS_IS_A_DIR\\*", "C:\\THIS_IS_A_DIR\\somefile")] // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/20
        [InlineData("/DIR1/*/*", "/DIR1/DIR2/file.txt")]  // Regression Test for https://github.com/dazinator/DotNet.Glob/issues/21
        [InlineData("~/*~3", "~/abc123~3")]  // Regression Test for https://github.com/dazinator/DotNet.Glob/pull/15  
        public void Match(string pattern, params string[] testStrings)
        {
            var glob = Globbing.Glob.Parse(pattern);
            foreach (var testString in testStrings)
            {
                var match = glob.Match(testString);
                Assert.True(match.Success);
            }
        }

        [Fact]
        public void To_String_Returns_Pattern()
        {
            var pattern = "p?th/*a[bcd]b[e-g]/**/a[1-4][!wxyz][!a-c][!1-3].*";
            var glob = Globbing.Glob.Parse(pattern);
            var resultPattern = glob.ToString();
            Assert.Equal(pattern, resultPattern);
        }   


      


      
    }
}

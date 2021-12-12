using System;
using System.Collections.Generic;
using Epsilon.Model.Resource;
using Xunit;

namespace Epsilon.Model.Tests
{
    public class ResourceServiceTest
    {
        [Fact]
        public void TestDefault()
        {
            var langues = new Dictionary<string, double>();
            langues.Add("fr-FR", 0.9);
            langues.Add("en-US", 0.8);
            langues.Add("en", 0.7);
            var lang = ResourceService.GetBestLCID(langues);

            Assert.True(lang == 12, "best culutre should be fr.");
        }

        [Fact]
        public void TestFrSecondPlace()
        {
            var langues = new Dictionary<string, double>();
            langues.Add("fr-FR", 0.9);
            langues.Add("en-US", 0.8);
            langues.Add("es", 1);
            var lang = ResourceService.GetBestLCID(langues);

            Assert.True(lang == 12, "best culutre should be fr.");
        }

        [Fact]
        public void TestEmpty()
        {
            var langues = new Dictionary<string, double>();
            var lang = ResourceService.GetBestLCID(langues);

            Assert.True(lang == 9, "default culture must be en.");
        }

        [Fact]
        public void TestNotSupported()
        {
            var langues = new Dictionary<string, double>();
            langues.Add("es", 1);
            var lang = ResourceService.GetBestLCID(langues);

            Assert.True(lang == 9, "default culture must be en.");
        }
    }
}

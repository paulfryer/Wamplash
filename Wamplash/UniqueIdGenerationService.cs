using System;
using System.Text;

namespace Wamplash
{
    public static class UniqueIdGenerationService
    {
        private static readonly Random Random = new Random();


        public static long GenerateUniqueId()
        {
            var first = Random.Next(int.MaxValue);
            var second = Random.Next(int.MaxValue);

            string longString = first.ToString() + second.ToString();
            var id = BitConverter.ToInt32(Encoding.UTF8.GetBytes(longString), 0);



            return id;
        }
    }
}
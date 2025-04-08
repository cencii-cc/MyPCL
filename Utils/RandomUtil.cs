using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPCL.Utils
{
    /// <summary>
    /// 随机数相关
    /// </summary>
    public static class RandomUtil
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// 随机选择其一。
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        /// <param name="objects">要进行随机选择的集合</param>
        /// <returns>随机选择的元素</returns>
        public static T RandomOne<T>(ICollection<T> objects)
        {
            if (objects.Count == 0)
            {
                throw new ArgumentException("集合不能为空。", nameof(objects));
            }
            int index = RandomInteger(0, objects.Count - 1);
            int currentIndex = 0;
            foreach (T item in objects)
            {
                if (currentIndex == index)
                {
                    return item;
                }
                currentIndex++;
            }
            // 正常情况下不会执行到这里，但为了满足语法要求添加返回语句
            throw new InvalidOperationException("无法从集合中选择元素。");
        }

        /// <summary>
        /// 取随机整数。
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（包含）</param>
        /// <returns>随机整数</returns>
        public static int RandomInteger(int min, int max)
        {
            return (int)Math.Floor((max - min + 1) * random.NextDouble()) + min;
        }

        /// <summary>
        /// 将数组随机打乱。
        /// </summary>
        /// <typeparam name="T">数组元素的类型</typeparam>
        /// <param name="array">要打乱的数组</param>
        /// <returns>打乱后的数组</returns>
        public static IList<T> Shuffle<T>(IList<T> array)
        {
            List<T> shuffledList = new List<T>();
            List<T> tempList = new List<T>(array);
            while (tempList.Count > 0)
            {
                int index = RandomInteger(0, tempList.Count - 1);
                shuffledList.Add(tempList[index]);
                tempList.RemoveAt(index);
            }
            return shuffledList;
        }

    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _0_Game.Dev.Scripts.Helper
{
    public static class Extension
    {
        private static readonly Dictionary<float, WaitForSeconds> WaitMap = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetWaitForSeconds(float duration)
        {
            if (WaitMap.TryGetValue(duration, out var waitForSeconds))
            {
                return waitForSeconds;
            }

            WaitMap.Add(duration, new WaitForSeconds(duration));

            return WaitMap[duration];
        }

        public static void DisableChilds(this Transform tr)
        {
            for (var i = tr.childCount - 1; i >= 0; i--)
            {
                tr.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static bool IsPointerOverUIElement(Vector3 point)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = point;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0;
        }

        /// <summary>
        /// Shuffle the list based on Fisher-Yates algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            System.Random rnd = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// Shuffle the array based on Fisher-Yates algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void Shuffle<T>(this T[] array)
        {
            System.Random rnd = new System.Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }

      

        public static Vector2 WorldToScreenPoint(Camera camera,RectTransform canvasRectTransform , Vector3 worldPoint)
        {
            var screenPos = camera.WorldToScreenPoint(worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPos,
                null, 
                out var localPoint
            );
            return localPoint;
        }
    }
}
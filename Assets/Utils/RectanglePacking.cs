using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Utils 
{
    public static class RectanglePacking
    {
        public enum Justification {
            Left,
            Center,
            Right
        }

        public class RectangleArrangement {
            public Vector2 dimensions;
            public List<(Vector2, Vector2)> children;

            public float fillRatio() {
                return children.Sum(x => x.Item2.x * x.Item2.y) / (dimensions.x * dimensions.y);
            }
        }
        public static IEnumerable<T> enumFlat<T>(this IEnumerable<IEnumerable<T>> l) {
            foreach (var sub_list in l) {
                foreach (var value in sub_list) {
                    yield return value;
                }
            }
        }

        private static float size(IEnumerable<Vector2> rectangles) {
            float s = 0;
            foreach (var v in rectangles) {
                s += v.x * v.y;
            }
            return s;
        }

        private static float fillRatio(IEnumerable<IEnumerable<Vector2>> rectangles, float aspectRatio) {
            float width = 0;
            float height = 0;
            foreach (var row in rectangles) {
                float rowWidth = 0;
                float rowHeight = 0;
                foreach (var r in row) {
                    rowWidth += r.x;
                    rowHeight = Mathf.Max(rowHeight, r.y);
                }
                width = Mathf.Max(width, rowWidth);
                height += rowHeight;
            }

            float frameWidth = Mathf.Max(width, aspectRatio * height);
            float frameHeight = Mathf.Max(height, frameWidth / aspectRatio);

            return size(rectangles.enumFlat()) / (frameWidth * frameHeight);
        }
        private static IEnumerable<List<int>> possibleRows(int n, int rows, int cols) {
            if (n == 0) {
                yield return new List<int>();
            }

            if (rows == 0 || cols == 0) {
                yield break;
            }

            if (rows > 0 && cols > 0 && rows * cols < n) {
                yield break;
            }
            
            int maxCols = cols;
            if (cols < 0) maxCols = n;
            int remainingRows = rows - 1;
            if (rows < 0) remainingRows = -1;

            for (int m = Math.Min(n, maxCols); m > 0; m--) {
                List<int> first = new List<int>();
                first.Add(m);
                foreach (var tail in possibleRows(n - m, remainingRows, cols)) {
                    yield return first.Concat(tail).ToList();
                }
            }
        }
        
        private static List<List<Vector2>> placeRectangles(List<int> arrangement, List<Vector2> rectangles) {
            List<List<Vector2>> placement = new List<List<Vector2>>();

            int i = 0;
            foreach (int n in arrangement) {
                List<Vector2> row = new List<Vector2>();
                int count = n;
                while (count > 0) {
                    row.Add(rectangles[i]);
                    i++;
                    count--;
                }
                placement.Add(row);
            }

            return placement;
        }

        private static RectangleArrangement arrangeRectangles(List<List<Vector2>> placement, float aspectRatio, 
                                                                Justification hJustification, Justification vJustification) {
            RectangleArrangement arrangement = new RectangleArrangement();
            List<List<(Vector2, Vector2)>> children = new List<List<(Vector2, Vector2)>>();

            float width = 0;
            float height = 0;
            foreach (var row in placement) {
                List<(Vector2, Vector2)> childRow = new List<(Vector2, Vector2)>();
                float rowWidth = 0;
                float rowHeight = 0;
                foreach (var r in row) {
                    childRow.Add((
                        new Vector2(rowWidth, height), 
                        new Vector2(r.x, r.y)
                    ));
                    rowWidth += r.x;
                    rowHeight = Mathf.Max(rowHeight, r.y);
                }
                width = Mathf.Max(width, rowWidth);
                height += rowHeight;
                children.Add(childRow);
            }

            float frameWidth = Mathf.Max(width, aspectRatio * height);
            float frameHeight = Mathf.Max(height, frameWidth / aspectRatio);

            foreach (var childRow in children) {
                float rowWidth = childRow.Sum(xywh => xywh.Item2.x);
                float rowHeight = childRow.Max(xywh => xywh.Item2.y);
                
                float xShift = 0;;
                switch (hJustification) {
                    case Justification.Left: xShift = 0; break;
                    case Justification.Center: xShift = (frameWidth - rowWidth) / 2; break;
                    case Justification.Right: xShift = frameWidth - rowWidth; break;
                }

                for (int i = 0; i < childRow.Count; i++) {
                    float yShift = 0;;
                    switch (vJustification) {
                        case Justification.Left: yShift = 0; break;
                        case Justification.Center: yShift = (rowHeight - childRow[i].Item2.y) / 2; break;
                        case Justification.Right: yShift = rowHeight - childRow[i].Item2.y; break;
                    }
                    Vector2 shift = new Vector2(xShift, yShift);
                    childRow[i] = (childRow[i].Item1 + shift, childRow[i].Item2);
                }
            }

            arrangement.children = children.SelectMany(x => x).ToList();
            arrangement.dimensions = new Vector2(frameWidth, frameHeight);
            return arrangement;
        }

        public static RectangleArrangement Arrange(List<Vector2> rectangles, float outAspect,
                                                        int rows = -1, int cols = -1,
                                                        Justification hJustification = Justification.Left,
                                                        Justification vJustification = Justification.Left) {
            float bestRatio = -1;
            RectangleArrangement bestArrangement = null;
            foreach (List<int> rowSizes in possibleRows(rectangles.Count, rows, cols)) {
                RectangleArrangement arrangement = 
                    arrangeRectangles(
                        placeRectangles(rowSizes, rectangles),
                        outAspect,
                        hJustification,
                        vJustification
                );
                float ratio = arrangement.fillRatio();
                if (ratio > bestRatio) {
                    bestArrangement = arrangement;
                    bestRatio = ratio;
                }
            }
            return bestArrangement;
        }

    }
}

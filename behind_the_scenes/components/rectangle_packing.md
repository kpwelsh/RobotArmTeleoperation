---
layout: default
title: Rectangle Packing
parent: Components
grand_parent: Behind the Scenes
math: mathjax
mermaid: True
---

# Usage

The ```RectanglePacking``` class in the ```Utils``` namespace provides a simple method for arranging a list of rectangles within a larger rectanlge. This is not a particularly efficient algorithm, so it is recommended to generate a single ```RectangleArrangement``` for a give configuration and reuse it. 

To generate an arrangement, all you need to do it call ```RectanglePacking.arrangeRectangles``` with a list of dimensions of the rectangles and a desired collage aspect ratio. You will then receive a ```RectangleArrangement``` struct that contains the total size of the arrangement (in the same units used to specify the original rectangles) and the absolute positions of each rectangle. Optionally, you can specify the horizontal and vertical justification to determine the placement when empty space is unavoidable.

{% highlight csharp %}
public class RectangleArrangement {
    public Vector2 dimensions;
    public List<(Vector2, Vector2)> children; // Top-left and bottom-right coordinates of the arranged rectangle

    public float fillRatio() {
        return children.Sum(x => x.Item2.x * x.Item2.y) / (dimensions.x * dimensions.y);
    }
}
{% endhighlight %}

The list of rectangles has the same length of the list provided, and each return rectangle represents the placement position of the input rectangle at the same index.

# Techincal Details

Generating an optimal [rectangle packing](https://en.wikipedia.org/wiki/Rectangle_packing) is in general an NP-hard problem. The algorithm implemented here scales very poorly, and in practice becomes impractical after ~30 rectangles. To make it manageable, several assumptions are made:

1. Rectangles cannot be rotated.
2. Rectangles cannot be scaled. 
3. The order in which the rectangles are provided is the order in which they should appead (from top-left to bottom-right).

With these assumptions, it seeks to break the input list into rows that result in the "best packed" configuration, as measured by the fraction of unfilled space in the resulting arrangement. With this goal, it simply tries all of the allowed configurations and chooses the best one.


{% highlight csharp %}
public static RectangleArrangement Arrange(List<Vector2> rectangles, float outAspect,
                                                int rows = -1, int cols = -1,
                                                Justification hJustification = Justification.Left,
                                                Justification vJustification = Justification.Left) {
    float bestRatio = -1;
    RectangleArrangement bestArrangement = null;
    // Iterate over all of the possible combinations
    // Becuase of assumption 1,2, and 3, simply specifying the number of rectangles
    // uniquely specifies the arrangement.
    foreach (List<int> rowSizes in possibleRows(rectangles.Count, rows, cols)) {
        // For each one, generate the specific rectangle arrangement
        // This computes the top left points as well as the width and height of each rectangle
        // as well as the dimensions of the minimum bounding rectangle
        RectangleArrangement arrangement = 
            arrangeRectangles(
                placeRectangles(rowSizes, rectangles),
                outAspect,
                hJustification,
                vJustification
        );
        // If this is a good fill ratio, then store it as the best
        // arrangement
        float ratio = arrangement.fillRatio();
        if (ratio > bestRatio) {
            bestArrangement = arrangement;
            bestRatio = ratio;
        }
    }
    return bestArrangement;
}
{% endhighlight %}
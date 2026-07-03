using System.ComponentModel;
using Microsoft.Xna.Framework;
using SimplePlatformer.Entities;

namespace SimplePlatformer.Systems;
public static class CollisionSystem
{
    private static readonly List<Solid> _solids = new();

    /// <summary>
    /// Returns whether there is a solid in the bounds.
    /// </summary>
    /// <param name="bounds">A Rectangle of the bounds to check for</param>
    public static bool OverlapsSolid(Rectangle bounds)
    {
        foreach (var solid in _solids)
        {
            if (bounds.Intersects(solid.Bounds))
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    ///  Returns the first solid in the list that overlaps the given bounds, or null if none do.
    /// </summary>
    /// <param name="bounds">A Rectangle of the bounds to check for</param>
    public static Solid? FirstOverlap(Rectangle bounds)
    {
        foreach (var solid in _solids)
        {
            if (bounds.Intersects(solid.Bounds))
            {
                return solid;
            }
        }
        return null;
    }

    /// <summary>
    /// Clears all solids from the collision system.
    /// </summary>
    // maybe also need mem management? idk how c# works for that
    public static void Clear()
    {
        _solids.Clear();
    }

    
    /// <summary>
    /// Adds a solid to the collision system.
    /// </summary>
    /// <param name="solid">The solid to remove.</param>
    public static void Add(Solid solid)
    {
        _solids.Add(solid);
    }

    /// <summary>
    /// Removes a solid from the collision system.
    /// </summary>
    /// <param name="solid">The solid to remove.</param>
    //should replace with faster algo at some point ig
    public static void Remove(Solid solid)
    {
        _solids.Remove(solid);
    }
}

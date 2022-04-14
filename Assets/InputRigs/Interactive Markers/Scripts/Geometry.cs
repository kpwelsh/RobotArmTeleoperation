using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LineSegment {
    public Vector3 A {
        get {
            return Origin;
        }
    }
    public Vector3 B {
        get {
            return Origin + Direction * Mathf.Min(Length, 100);
        }
    }
    public Vector3 Origin;
    public Vector3 Direction;
    public float Length;
    public Ray Ray {
        get {
            return new Ray(Origin, Direction);
        }
    }
    public LineSegment(Vector3 A, Vector3 B) {
        Origin = A;
        Direction = B - A;
        Length = Direction.magnitude;
        Direction /= Length;
    }
    public LineSegment(Vector3 origin, Vector3 direction, float length) {
        Origin = origin;
        Direction = direction;
        Length = length;
    }
    public LineSegment(Ray ray, float length) {
        Origin = ray.origin;
        Direction = ray.direction;
        Length = length;
    }

    public (Vector3, Vector3) ClosestPoints(Ray ray) {
        (Vector3 a, Vector3 b) = this.Ray.ClosestPoints(ray);
        return (Project(a), b);
    }
    public (Vector3, Vector3) ClosestPoints(LineSegment other) {
        (Vector3 a, Vector3 b) = this.Ray.ClosestPoints(other.Ray);
        return (Project(a), other.Project(b));
    }
    public Vector3 Project(Vector3 p) {
        return Origin + Direction * Mathf.Clamp(Vector3.Dot(p - Origin, Direction), 0, Length);
    }
}

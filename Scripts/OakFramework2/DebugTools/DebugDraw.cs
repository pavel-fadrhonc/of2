﻿using UnityEngine;
using Primitives;
using CollisionLib;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class DebugDraw
{
    public static void Circle(Vector3 center, Vector3 normal, float radius, Color color)
    {
        Vector3 v1;
        Vector3 v2;
        CalculatePlaneVectorsFromNormal(normal, out v1, out v2);
        CircleInternal(center, v1, v2, radius, color);
    }

    public static void Sphere(Vector3 center, float radius)
    {
        Sphere(center, radius, Color.white);
    }

    public static void Sphere(Vector3 center, float radius, Color color, float duration = 0)
    {
        CircleInternal(center, Vector3.right, Vector3.up, radius, color, duration);
        CircleInternal(center, Vector3.forward, Vector3.up, radius, color, duration);
        CircleInternal(center, Vector3.right, Vector3.forward, radius, color, duration);
    }

    public static void Prim(sphere sphere, Color color, float duration = 0)
    {
        Sphere(sphere.center, sphere.radius, color, duration);
    }
    
    public static void Prim(ray ray, Color color, float duration = 0)
    {
        Debug.DrawLine(ray.origin, ray.origin + ray.direction*1000, color, duration);
        Sphere(ray.origin, 0.03f, color, duration);
    }

    public static void Cross(Vector3 center, float size, Color color, Vector3 normal, float duration = 0)
    {
        Cross(center, size, size, color, normal, duration);
    }

    public static void Cross(Vector3 center, float sizeX, float sizeY, Color color, Vector3 normal, float duration = 0)
    {
        var rightVector = Vector3.Cross(Vector3.up, normal).normalized;
        var upVector = Vector3.Cross(rightVector, normal).normalized;
        Debug.DrawLine(center - 0.5f * sizeX * rightVector, center + 0.5f * sizeX * rightVector, color);
        Debug.DrawLine(center - 0.5f * sizeY * upVector, center + 0.5f * sizeY * upVector, color);
    }
    
    public static void Capsule(Vector3 center, Vector3 dir, float radius, float height, Color color, float duration = 0)
    {
        var cylinderHeight = height - radius*2;
        var v = Vector3.Angle(dir,Vector3.up) > 0.001 ? Vector3.Cross(dir, Vector3.up) : Vector3.Cross(dir, Vector3.left);
        
        CircleInternal(center + dir*cylinderHeight*0.5f, dir, v, radius, color, duration);
        CircleInternal(center - dir*cylinderHeight*0.5f, dir, v, radius, color, duration);
        
        Cylinder(center, dir, radius, cylinderHeight * 0.5f, color, duration);
    }

    public static void Prim(capsule capsule, Color color, float duration = 0)
    {
        var v = capsule.p2 - capsule.p1;
        Capsule(capsule.p1 + v*0.5f, math.normalize(v), capsule.radius, math.length(v) + 2*capsule.radius, color, duration);
    }
    
    public static void Cylinder(Vector3 center, Vector3 normal, float radius, float halfHeight, Color color, float duration = 0)
    {
        Vector3 v1;
        Vector3 v2;
        CalculatePlaneVectorsFromNormal(normal, out v1, out v2);

        var offset = normal * halfHeight;
        CircleInternal(center - offset, v1, v2, radius, color, duration);
        CircleInternal(center + offset, v1, v2, radius, color, duration);

        const int segments = 20;
        float arc = Mathf.PI * 2.0f / segments;
        for (var i = 0; i < segments; i++)
        {
            Vector3 p = center + v1 * Mathf.Cos(arc * i) * radius + v2 * Mathf.Sin(arc * i) * radius;
            Debug.DrawLine(p - offset, p + offset, color, duration);
        }
    }

    public static void Prim(box box, Color color, float duration = 0)
    {
        var size = box.size;
        var axisX = mul(box.rotation,float3(1,0,0)) * size.x;   
        var axisY = mul(box.rotation,float3(0,1,0)) * size.y;
        var axisZ = mul(box.rotation,float3(0,0,1)) * size.z;

        var A = box.center + (axisX + axisY + axisZ)*0.5f;
        var B = A - axisY;
        var C = B - axisZ;
        var D = C + axisY;
        
        var E = A - axisX;
        var F = B - axisX;
        var G = C - axisX;
        var H = D - axisX;
        
        Debug.DrawLine(A, B, color, duration);
        Debug.DrawLine(B, C, color, duration);
        Debug.DrawLine(C, D, color, duration);
        Debug.DrawLine(D, A, color, duration);

        Debug.DrawLine(E, F, color, duration);
        Debug.DrawLine(F, G, color, duration);
        Debug.DrawLine(G, H, color, duration);
        Debug.DrawLine(H, E, color, duration);
        
        Debug.DrawLine(A, E, color, duration);
        Debug.DrawLine(B, F, color, duration);
        Debug.DrawLine(C, G, color, duration);
        Debug.DrawLine(H, D, color, duration);
    }
    
    static void CircleInternal(Vector3 center, Vector3 v1, Vector3 v2, float radius, Color color, float duration = 0)
    {
        const int segments = 20;
        float arc = Mathf.PI * 2.0f / segments;
        Vector3 p1 = center + v1 * radius;
        for (var i = 1; i <= segments; i++)
        {
            Vector3 p2 = center + v1 * Mathf.Cos(arc * i) * radius + v2 * Mathf.Sin(arc * i) * radius;
            Debug.DrawLine(p1, p2, color, duration);
            p1 = p2;
        }
    }

    static void CalculatePlaneVectorsFromNormal(Vector3 normal, out Vector3 v1, out Vector3 v2)
    {
        if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.99)
        {
            v1 = Vector3.Cross(Vector3.up, normal).normalized;
            v2 = Vector3.Cross(normal, v1);
        }
        else
        {
            v1 = Vector3.Cross(Vector3.left, normal).normalized;
            v2 = Vector3.Cross(normal, v1);
        }
    }
}

﻿using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public static class CameraHelpers
    {
        private static RenderTexture screenshotRenderTexture;

        /// <summary>
        /// Takes a snapshot of the current frame and stores it in a texture2D
        /// </summary>
        /// <param name="camera">Camera to take a screenshot from</param>
        /// <param name="width">Width of the resulting texture</param>
        /// <param name="height">Height of the resulting texture</param>
        /// <param name="depth">Color depth of the resulting texture</param>
        /// <returns>A screenshot</returns>
        public static Texture2D TakeScreenshot(this Camera camera, int width = 2048, int height = 2048, int depth = 32)
        {
            RenderTexture tempRenderTexture = camera.targetTexture; //In case user has a render texture set, remember it
            if (screenshotRenderTexture != null && (screenshotRenderTexture.width != width || screenshotRenderTexture.height != height || screenshotRenderTexture.depth != depth))
            {
                GameObject.Destroy(screenshotRenderTexture);
                screenshotRenderTexture = null;
            }
            if (screenshotRenderTexture == null)
            {
                screenshotRenderTexture = new RenderTexture(width, height, depth);
            }
            camera.targetTexture = screenshotRenderTexture;
            camera.Render();

            Texture2D saveTexture = new Texture2D(screenshotRenderTexture.width, screenshotRenderTexture.height);
            RenderTexture.active = screenshotRenderTexture;
            Rect renderRect = new Rect(0, 0, screenshotRenderTexture.width, screenshotRenderTexture.height);
            saveTexture.ReadPixels(renderRect, 0, 0); //function reads from the active render texture

            camera.targetTexture = tempRenderTexture; //Return user's render texture if any
            return saveTexture;
        }
        //// <summary>
        /// Returns the forward distance needed to place the camera to fit a
        /// specific world width on screen in relation to the camera's perspective.
        /// </summary>
        /// <param name="camera">The camera to apply the calculation to</param>
        /// <param name="worldWidth">The requested world width</param>
        /// <param name="debug">Show frustum lines in editor</param>
        /// <returns>Camera forward distance</returns>
        public static float PerspectiveDistanceFromWidth(this Camera camera, float worldWidth, bool debug = false)
        {
            Vector2 nearDimensions = camera.PerspectiveFrustumAtNear(debug);
            Vector2 farDimensions = camera.PerspectiveFrustumAtFar(debug);
            float slope = ((camera.farClipPlane - camera.nearClipPlane) / (farDimensions.x - nearDimensions.x));
            float intercept = camera.nearClipPlane - slope * nearDimensions.x;
            return slope * worldWidth + intercept + camera.nearClipPlane;
        }
        /// <summary>
        /// Returns the forward distance needed to place the camera to fit a
        /// specific world height on screen in relation to the camera's perspective.
        /// </summary>
        /// <param name="camera">The camera to apply the calculation to</param>
        /// <param name="worldHeight">The requested world height</param>
        /// <param name="debug">Show frustum lines in editor</param>
        /// <returns>Camera forward distance</returns>
        public static float PerspectiveDistanceFromHeight(this Camera camera, float worldHeight, bool debug = false)
        {
            Vector2 nearDimensions = camera.PerspectiveFrustumAtNear(debug);
            Vector2 farDimensions = camera.PerspectiveFrustumAtFar(debug);
            float slope = ((camera.farClipPlane - camera.nearClipPlane) / (farDimensions.y - nearDimensions.y));
            float intercept = camera.nearClipPlane - slope * nearDimensions.y;
            return slope * worldHeight + intercept + camera.nearClipPlane;
        }
        /// <summary>
        /// Gets the world plane width and height of the camera's perpective
        /// at a specified distance.
        /// </summary>
        /// <param name="fieldOfView">The camera's field of view</param>
        /// <param name="frustumDistance">Distance of the plane from the camera</param>
        /// <param name="aspect">The camera's aspect ratio</param>
        /// <returns>Perspective plane dimensions</returns>
        public static Vector2 PerspectiveFrustum(float fieldOfView, float frustumDistance, float aspect)
        {
            float frustumHeight = 2.0f * frustumDistance * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * aspect;
            return new Vector2(frustumWidth, frustumHeight);
        }
        /// <summary>
        /// Gets the world plane width and height of the camera's perpective
        /// at a specified distance.
        /// </summary>
        /// <param name="distance">Distance of the plane from the camera</param>
        /// <returns>Perspective plane dimensions</returns>
        public static Vector2 PerspectiveFrustum(this Camera camera, float distance)
        {
            return PerspectiveFrustum(camera.fieldOfView, distance, camera.aspect);
        }
        /// <summary>
        /// Gets the world plane width and height of the camera's perpective
        /// at the far clipping plane.
        /// </summary>
        /// <param name="camera">The camera to calculate the dimensions for</param>
        /// <param name="debug">Show frustum lines in editor</param>
        /// <returns>Perspective plane dimensions</returns>
        public static Vector2 PerspectiveFrustumAtFar(this Camera camera, bool debug = false)
        {
            Vector2 frustumDimensions = camera.PerspectiveFrustum(camera.farClipPlane);

            if (debug)
            {
                Vector3 farClipCenter = camera.transform.position + camera.farClipPlane * camera.transform.forward;
                Vector3 halfHeight = camera.transform.up * frustumDimensions.y / 2f;
                Vector3 halfWidth = camera.transform.right * frustumDimensions.x / 2f;
                Debug.DrawLine(farClipCenter + halfHeight - halfWidth, farClipCenter + halfHeight + halfWidth, Color.red, 1);
                Debug.DrawLine(farClipCenter + halfWidth - halfHeight, farClipCenter + halfWidth + halfHeight, Color.green, 1);
            }

            return frustumDimensions;
        }
        /// <summary>
        /// Gets the world plane width and height of the camera's perpective
        /// at the near clipping plane.
        /// </summary>
        /// <param name="camera">The camera to calculate the dimensions for</param>
        /// <param name="debug">Show frustum lines in editor</param>
        /// <returns>Perspective plane dimensions</returns>
        public static Vector2 PerspectiveFrustumAtNear(this Camera camera, bool debug = false)
        {
            Vector2 frustumDimensions = camera.PerspectiveFrustum(camera.nearClipPlane);

            if (debug)
            {
                Vector3 nearClipCenter = camera.transform.position + camera.nearClipPlane * camera.transform.forward;
                Vector3 halfHeight = camera.transform.up * frustumDimensions.y / 2f;
                Vector3 halfWidth = camera.transform.right * frustumDimensions.x / 2f;
                Debug.DrawLine(nearClipCenter + halfHeight - halfWidth, nearClipCenter + halfHeight + halfWidth, Color.red, 1);
                Debug.DrawLine(nearClipCenter + halfWidth - halfHeight, nearClipCenter + halfWidth + halfHeight, Color.green, 1);
            }

            return frustumDimensions;
        }
        /// <summary>
        /// Creates a rect with the given aspect ratio that contains both points
        /// </summary>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        /// <param name="aspect">Required aspect ratio</param>
        /// <returns>A rect with the proper aspect ratio</returns>
        public static Rect Aspectify(Vector2 pointA, Vector2 pointB, float aspect)
        {
            Vector2 min = new Vector2(Mathf.Min(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y));
            Vector2 max = new Vector2(Mathf.Max(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y));

            Rect toAspectify = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            Vector2 maxedSize = new Vector2(Mathf.Max(toAspectify.height * aspect, toAspectify.width), Mathf.Max(toAspectify.width / aspect, toAspectify.height));
            return toAspectify.ResizeFromCenter(maxedSize);
        }
        /// <summary>
        /// Creates a rect that contains all points given while all points have the requested padding
        /// </summary>
        /// <param name="padding">Amount to cushion by</param>
        /// <param name="currentPoints">Points to include</param>
        /// <returns>A rect that contains all padded points</returns>
        public static Rect PaddedMinMax(float padding, params Vector3[] currentPoints)
        {
            Rect paddedRect = new Rect();
            if (currentPoints != null && currentPoints.Length > 1)
                paddedRect = currentPoints.Select(point => new Rect(point.x, point.z, 0, 0).ResizeFromCenter(padding, padding)).Aggregate(RectHelpers.Grow);
            else if (currentPoints != null && currentPoints.Length > 0)
                paddedRect = new Rect(currentPoints[0].x, currentPoints[0].z, 0, 0).ResizeFromCenter(padding, padding);

            return paddedRect;
        }

    }
}
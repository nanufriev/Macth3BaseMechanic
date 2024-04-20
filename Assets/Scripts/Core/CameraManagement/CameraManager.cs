using UnityEngine;

namespace Core.CameraManagement
{
    public class CameraManager
    {
        private const float MARGIN = 1f;
        private const float CAMERA_Z = -10f;
        private readonly Camera _camera;

        public CameraManager(Camera camera)
        {
            _camera = camera;
        }

        public void AdjustCameraToGrid(int gridWidth, int gridHeight)
        {
            var effectiveGridWidth = gridWidth + MARGIN * 2;
            var effectiveGridHeight = gridHeight + MARGIN * 2;

            var gridAspectRatio = (float)gridWidth / gridHeight;
            var screenAspectRatio = (float)Screen.width / Screen.height;

            var orthographicSize = screenAspectRatio >= gridAspectRatio
                ? effectiveGridHeight / 2f
                : effectiveGridWidth / (2f * screenAspectRatio);

            _camera.orthographicSize = orthographicSize;
            _camera.transform.position = new Vector3(gridWidth / 2f - 0.5f, gridHeight / 2f - 0.5f, CAMERA_Z);
        }
    }
}
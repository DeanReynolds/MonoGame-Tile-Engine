using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Terraria_World
{
    public class Camera
    {
        public float X
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                UpdateXPositionInTransform();
            }
        }
        public float Y
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                UpdateYPositionInTransform();
            }
        }
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdatePositionInTransform();
            }
        }
        public float Angle
        {
            get { return _angle; }
            set
            {
                _rotM11 = (float)System.Math.Cos(-(_angle = value));
                _rotM12 = (float)System.Math.Sin(-_angle);
                UpdateScaleInTransform();
                UpdatePositionInTransform();
            }
        }
        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                UpdateScaleInTransform();
                UpdatePositionInTransform();
            }
        }
        public Vector2 ScreenSize
        {
            get { return _screenSize; }
            set
            {
                _screenSize = value;
                _screenCenter.X = (_screenSize.X / 2f);
                _screenCenter.Y = (_screenSize.Y / 2f);
                UpdatePositionInTransform();
                _projection.M11 = (float)(2d / _screenSize.X);
                _projection.M22 = (float)(2d / -_screenSize.Y);
                _projection.M41 = (float)((double)_screenSize.X / -_screenSize.X);
                _projection.M42 = (float)((double)_screenSize.Y / _screenSize.Y);
            }
        }

        public Matrix Transform { get { return _transform; } }
        public Vector2 MousePosition { get { return _mousePosition; } }
        public Matrix Projection { get { return _projection; } }

        private Vector2 _position;
        private float _angle;
        private Vector2 _screenSize;
        private Vector2 _screenCenter;
        private float _rotM11;
        private float _rotM12;
        private float _rotX1;
        private float _rotY1;
        private float _rotX2;
        private float _rotY2;
        private float _scale;
        private double _n27;
        private Matrix _transform;
        private Matrix _transformInvert;
        private Vector2 _mousePosition;
        private Matrix _projection;

        public Camera(float angle = 0, float scale = 1) : this(Vector2.Zero, angle, scale) { }
        public Camera(Vector2 position, float angle = 0, float scale = 1)
        {
            _position.X = position.X;
            _position.Y = position.Y;
            _rotM11 = (float)System.Math.Cos(-(_angle = angle));
            _rotM12 = (float)System.Math.Sin(-_angle);
            _scale = scale;
            _screenSize = new Vector2(Game1.Viewport.Width, Game1.Viewport.Height);
            _screenCenter.X = (_screenSize.X / 2f);
            _screenCenter.Y = (_screenSize.Y / 2f);
            _transform = new Matrix
            {
                M33 = 1,
                M44 = 1
            };
            _transformInvert = new Matrix
            {
                M33 = 1,
                M44 = 1
            };
            UpdateScaleInTransform();
            UpdatePositionInTransform();
            _projection = new Matrix
            {
                M11 = (float)(2d / _screenSize.X),
                M22 = (float)(2d / -_screenSize.Y),
                M33 = -1,
                M41 = -1,
                M42 = 1,
                M44 = 1
            };
        }

        public void UpdateMousePosition(MouseState? mouseState = null)
        {
            if (!mouseState.HasValue)
                mouseState = Mouse.GetState();
            float mouseX = mouseState.Value.Position.X;
            float mouseY = mouseState.Value.Position.Y;
            _mousePosition.X = ((mouseX * _transformInvert.M11) + (mouseY * _transformInvert.M21) + _transformInvert.M41);
            _mousePosition.Y = ((mouseX * _transformInvert.M12) + (mouseY * _transformInvert.M22) + _transformInvert.M42);
        }

        private void UpdateXPositionInTransform()
        {
            float m41 = (-_position.X * _scale);
            _rotX1 = (m41 * _rotM11);
            _rotX2 = (m41 * _rotM12);
            _transform.M41 = ((_rotX1 + _rotY1) + _screenCenter.X);
            _transform.M42 = ((_rotX2 + _rotY2) + _screenCenter.Y);
            _transformInvert.M41 = (float)(-((double)_transform.M21 * -_transform.M42 - (double)_transform.M22 * -_transform.M41) * _n27);
            _transformInvert.M42 = (float)(((double)_transform.M11 * -_transform.M42 - (double)_transform.M12 * -_transform.M41) * _n27);
        }

        private void UpdateYPositionInTransform()
        {
            float m42 = (-_position.Y * _scale);
            _rotY1 = (m42 * -_rotM12);
            _rotY2 = (m42 * _rotM11);
            _transform.M41 = ((_rotX1 + _rotY1) + _screenCenter.X);
            _transform.M42 = ((_rotX2 + _rotY2) + _screenCenter.Y);
            _transformInvert.M41 = (float)(-((double)_transform.M21 * -_transform.M42 - (double)_transform.M22 * -_transform.M41) * _n27);
            _transformInvert.M42 = (float)(((double)_transform.M11 * -_transform.M42 - (double)_transform.M12 * -_transform.M41) * _n27);
        }

        private void UpdatePositionInTransform()
        {
            float m41 = (-_position.X * _scale);
            float m42 = (-_position.Y * _scale);
            _rotX1 = (m41 * _rotM11);
            _rotY1 = (m42 * -_rotM12);
            _rotX2 = (m41 * _rotM12);
            _rotY2 = (m42 * _rotM11);
            _transform.M41 = ((_rotX1 + _rotY1) + _screenCenter.X);
            _transform.M42 = ((_rotX2 + _rotY2) + _screenCenter.Y);
            _transformInvert.M41 = (float)(-((double)_transform.M21 * -_transform.M42 - (double)_transform.M22 * -_transform.M41) * _n27);
            _transformInvert.M42 = (float)(((double)_transform.M11 * -_transform.M42 - (double)_transform.M12 * -_transform.M41) * _n27);
        }

        public void UpdateScaleInTransform()
        {
            _transform.M11 = (_scale * _rotM11);
            _transform.M12 = (_scale * _rotM12);
            _transform.M21 = (_scale * -_rotM12);
            _transform.M22 = (_scale * _rotM11);
            _n27 = (1d / ((double)_transform.M11 * _transform.M22 + (double)_transform.M12 * -_transform.M21));
            _transformInvert.M11 = (float)(_transform.M22 * _n27);
            _transformInvert.M21 = (float)(-_transform.M21 * _n27);
            _transformInvert.M12 = (float)-(_transform.M12 * _n27);
            _transformInvert.M22 = (float)(_transform.M11 * _n27);
        }
    }
}
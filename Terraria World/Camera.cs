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
                _positionTranslation.M41 = -(_position.X = value);
                UpdatePositionInTransform();
            }
        }
        public float Y
        {
            get { return _position.Y; }
            set
            {
                _positionTranslation.M42 = -(_position.Y = value);
                UpdatePositionInTransform();
            }
        }
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _positionTranslation.M41 = -(_position.X = value.X);
                _positionTranslation.M42 = -(_position.Y = value.Y);
                UpdatePositionInTransform();
            }
        }
        public float Angle
        {
            get { return _angle; }
            set
            {
                _rotationZ.M22 = _rotationZ.M11 = (float)System.Math.Cos(-(_angle = value));
                _rotationZ.M21 = -(_rotationZ.M12 = (float)System.Math.Sin(-_angle));
                UpdateScaleInTransform();
                UpdatePositionInTransform();
            }
        }
        public float Scale
        {
            get { return _scale.M11; }
            set
            {
                _scale.M11 = _scale.M22 = value;
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
                _projection.M11 = (float)(2d / _screenSize.X);
                _projection.M22 = (float)(2d / -_screenSize.Y);
                _projection.M41 = (float)((double)_screenSize.X / -_screenSize.X);
                _projection.M42 = (float)((double)_screenSize.Y / _screenSize.Y);
                _screenTranslation.M41 = (_screenSize.X / 2f);
                _screenTranslation.M42 = (_screenSize.Y / 2f);
                UpdatePositionInTransform();
            }
        }

        public Matrix Transform { get { return _transform; } }
        public Vector2 MousePosition { get { return _mousePosition; } }
        public Matrix Projection { get { return _projection; } }

        private Vector2 _position;
        private float _angle;
        private Vector2 _screenSize;
        private Matrix _positionTranslation;
        private Matrix _rotationZ;
        private Matrix _scale;
        private double _n27;
        private Matrix _screenTranslation;
        private Matrix _transform;
        private Matrix _transformInvert;
        private Vector2 _mousePosition;
        private Matrix _projection;

        public Camera(float angle = 0, float scale = 1) : this(Vector2.Zero, angle, scale) { }
        public Camera(Vector2 position, float angle = 0, float scale = 1)
        {
            _positionTranslation = new Matrix
            {
                M11 = 1,
                M22 = 1,
                M33 = 1,
                M41 = -(_position.X = position.X),
                M42 = -(_position.Y = position.Y),
                M44 = 1
            };
            float rotVal1 = (float)System.Math.Cos(-(_angle = angle));
            float rotVal2 = (float)System.Math.Sin(-_angle);
            _rotationZ = new Matrix
            {
                M11 = rotVal1,
                M12 = rotVal2,
                M21 = -rotVal2,
                M22 = rotVal1,
                M33 = 1,
                M44 = 1
            };
            _scale = new Matrix
            {
                M11 = scale,
                M22 = scale,
                M33 = 1,
                M44 = 1
            };
            _screenSize = new Vector2(Game1.Viewport.Width, Game1.Viewport.Height);
            _screenTranslation = new Matrix
            {
                M11 = 1,
                M22 = 1,
                M33 = 1,
                M41 = (_screenSize.X / 2),
                M42 = (_screenSize.Y / 2),
                M44 = 1
            };
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

        private void UpdatePositionInTransform()
        {
            float m41 = (_positionTranslation.M41 * _scale.M11);
            float m42 = (_positionTranslation.M42 * _scale.M22);
            _transform.M41 = (((m41 * _rotationZ.M11) + (m42 * _rotationZ.M21)) + _screenTranslation.M41);
            _transform.M42 = (((m41 * _rotationZ.M12) + (m42 * _rotationZ.M22)) + _screenTranslation.M42);
            _transformInvert.M41 = (float)(-((double)_transform.M21 * -_transform.M42 - (double)_transform.M22 * -_transform.M41) * _n27);
            _transformInvert.M42 = (float)(((double)_transform.M11 * -_transform.M42 - (double)_transform.M12 * -_transform.M41) * _n27);
        }

        public void UpdateScaleInTransform()
        {
            _transform.M11 = ((_scale.M11 * _rotationZ.M11) + (_scale.M12 * _rotationZ.M21));
            _transform.M12 = ((_scale.M11 * _rotationZ.M12) + (_scale.M12 * _rotationZ.M22));
            _transform.M21 = ((_scale.M21 * _rotationZ.M11) + (_scale.M22 * _rotationZ.M21));
            _transform.M22 = ((_scale.M21 * _rotationZ.M12) + (_scale.M22 * _rotationZ.M22));
            _n27 = (1d / ((double)_transform.M11 * _transform.M22 + (double)_transform.M12 * -_transform.M21));
            _transformInvert.M11 = (float)(_transform.M22 * _n27);
            _transformInvert.M21 = (float)(-_transform.M21 * _n27);
            _transformInvert.M12 = (float)-(_transform.M12 * _n27);
            _transformInvert.M22 = (float)(_transform.M11 * _n27);
        }
    }
}
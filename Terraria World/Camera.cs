using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Terraria_World
{
    public class Camera
    {
        public float X
        {
            get { return _position.X; }
            set { _positionTranslation.M41 = -(_position.X = value); }
        }
        public float Y
        {
            get { return _position.Y; }
            set { _positionTranslation.M42 = -(_position.Y = value); }
        }
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _positionTranslation.M41 = -(_position.X = value.X);
                _positionTranslation.M42 = -(_position.Y = value.Y);
            }
        }
        public float Angle
        {
            get { return _angle; }
            set { _rotationZ = Matrix.CreateRotationZ(-(_angle = value)); }
        }
        public float Scale
        {
            get { return _scale.M11; }
            set { _scale.M11 = _scale.M22 = value; }
        }
        public Vector2 ScreenSize
        {
            get { return _screenSize; }
            set
            {
                _screenSize = value;
                _projection.M11 = (float)(2d / (_screenSize.X - 0d));
                _projection.M22 = (float)(2d / (0d - _screenSize.Y));
                _projection.M41 = (float)((0d + _screenSize.X) / (0d - _screenSize.X));
                _projection.M42 = (float)((0d + _screenSize.Y) / (_screenSize.Y - 0d));
            }
        }
        public Vector2 ScreenCenter
        {
            get { return _screenCenter; }
            set
            {
                _screenTranslation.M41 = _screenCenter.X = value.X;
                _screenTranslation.M42 = _screenCenter.Y = value.Y;
            }
        }

        public Matrix Transform { get; private set; }
        public Vector2 MousePosition { get; private set; }

        public Matrix Projection { get { return _projection; } }

        private Vector2 _position;
        private float _angle;
        private Vector2 _screenSize;
        private Vector2 _screenCenter;
        private Matrix _positionTranslation;
        private Matrix _rotationZ;
        private Matrix _scale;
        private Matrix _screenTranslation;
        private Matrix _transformInvert;
        private Matrix _projection;

        public Camera(float angle = 0, float scale = 1) : this(Vector2.Zero, angle, scale) { }
        public Camera(Vector2 position, float angle = 0, float scale = 1)
        {
            _positionTranslation = Matrix.CreateTranslation(-(_position.X = position.X), -(_position.Y = position.Y), 0);
            _angle = angle;
            _rotationZ = Matrix.CreateRotationZ(-angle);
            _scale = Matrix.CreateScale(scale, scale, 1);
            _screenCenter = new Vector2((Game1.VirtualWidth / 2f), (Game1.VirtualHeight / 2f));
            _screenTranslation = Matrix.CreateTranslation(_screenCenter.X, _screenCenter.Y, 0);
            UpdateTransform();
            _screenSize = new Vector2(Game1.VirtualWidth, Game1.VirtualHeight);
            _projection = Matrix.CreateOrthographicOffCenter(0, _screenSize.X, _screenSize.Y, 0, 0, 1);
        }

        public void UpdateMousePosition(MouseState? mouseState = null)
        {
            if (!mouseState.HasValue)
                mouseState = Mouse.GetState();
            Vector2 mousePos = mouseState.Value.Position.ToVector2();
            mousePos.X = (((mousePos.X / Game1.WindowWidth) * Game1.VirtualWidth) - Game1.ViewportX);
            mousePos.Y = (((mousePos.Y / Game1.WindowHeight) * Game1.VirtualHeight) - Game1.ViewportY);
            Vector2.Transform(ref mousePos, ref _transformInvert, out mousePos);
            MousePosition = mousePos;
        }

        public void UpdateTransform()
        {
            Transform = (_positionTranslation * _rotationZ * _scale * _screenTranslation);
            _transformInvert = Matrix.Invert(Transform);
        }
    }
}
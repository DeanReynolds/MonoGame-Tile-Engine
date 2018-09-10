using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Terraria_World
{
    public class Camera
    {
        public static Matrix CreateScreenTranslation(float width, float height) { return Matrix.CreateTranslation((width / 2), (height / 2), 0); }
        public static Matrix CreateProjection(float width, float height) { return Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, 1); }
        public static Matrix CreateTransform(Matrix position, Matrix rotationZ, Matrix scale, Matrix screenTranslation) { return (position * rotationZ * scale * screenTranslation); }

        public float X
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                PositionTranslation = Matrix.CreateTranslation(-_position);
                UpdateTransform();
            }
        }
        public float Y
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                PositionTranslation = Matrix.CreateTranslation(-_position);
                UpdateTransform();
            }
        }
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                RotationZ = Matrix.CreateRotationZ(-_angle);
                UpdateTransform();
            }
        }
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                Scale = Matrix.CreateScale(new Vector3(_zoom, _zoom, 1));
                UpdateTransform();
            }
        }
        public Matrix Projection;
        public Matrix ScreenTranslation
        {
            get { return _screenTranslation; }
            set
            {
                _screenTranslation = value;
                UpdateTransform();
            }
        }

        public Matrix PositionTranslation { get; private set; }
        public Matrix RotationZ { get; private set; }
        public Matrix Scale { get; private set; }
        public Matrix Transform { get; private set; }
        public Vector2 MousePosition { get; private set; }

        private Vector3 _position;
        private float _angle;
        private float _zoom;
        private Matrix _screenTranslation;
        private Matrix _invert;

        public Camera(float angle = 0, float zoom = 1) : this(Vector2.Zero, angle, zoom) { }
        public Camera(Vector2 position, float angle = 0, float zoom = 1)
        {
            _position = new Vector3(position, 0);
            PositionTranslation = Matrix.CreateTranslation(-_position);
            _angle = angle;
            RotationZ = Matrix.CreateRotationZ(-angle);
            _zoom = zoom;
            Scale = Matrix.CreateScale(new Vector3(zoom, zoom, 1));
            _screenTranslation = CreateScreenTranslation(Screen.VirtualWidth, Screen.VirtualHeight);
            Projection = CreateProjection(Screen.VirtualWidth, Screen.VirtualHeight);
            UpdateTransform();
        }

        public void UpdateMousePosition(MouseState? mouseState = null)
        {
            if (!mouseState.HasValue)
                mouseState = Mouse.GetState();
            Vector2 mousePos = mouseState.Value.Position.ToVector2();
            mousePos.X = (((mousePos.X / Screen.WindowWidth) * Screen.VirtualWidth) - Screen.ViewportX);
            mousePos.Y = (((mousePos.Y / Screen.WindowHeight) * Screen.VirtualHeight) - Screen.ViewportY);
            Vector2.Transform(ref mousePos, ref _invert, out mousePos);
            MousePosition = mousePos;
        }

        public void UpdateTransform()
        {
            Transform = CreateTransform(PositionTranslation, RotationZ, Scale, ScreenTranslation);
            _invert = Matrix.Invert(Transform);
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SmoothMoveTest;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Vector2 position;
    private Vector2 velocity;

    private Vector2 targetPosition;

    private Texture2D appleTexture;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        position = new Vector2(0.0f, 0.0f);
        velocity = new Vector2(0.0f, 0.0f);

        targetPosition = new Vector2(100.0f, 100.0f);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        appleTexture = this.Content.Load<Texture2D>("apple");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // updating target pos
        if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
            Point targetPoint = Mouse.GetState().Position;
            targetPosition = new Vector2(targetPoint.X, targetPoint.Y);
        }

        // moving toward target pos
        (position, velocity) = PosUpdate2(position, velocity, targetPosition, (float)gameTime.ElapsedGameTime.TotalSeconds);

        base.Update(gameTime);
    }

    // acceleration with "speed limit" approach
    private (Vector2, Vector2) PosUpdate1(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        // step 1: acceleration is relative to distance to target
        Vector2 acceleration = (targetPosition - position) * 10.0f;
        velocity += (acceleration * timeElapsed);

        // step 2: speed is capped relative to distance to target
        float distance = (targetPosition - position).Length();
        float speed = velocity.Length();
        float maxSpeed = distance * 10.0f;
        if (speed >= maxSpeed) {
            velocity.Normalize();
            velocity *= maxSpeed;
        }

        // step 3: add velocity to position
        position += (velocity * timeElapsed);

        return (position, velocity);
    }

    // speed limit plus dampen vector rejection
    private (Vector2, Vector2) PosUpdate2(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        // step 1: acceleration is relative to distance to target
        Vector2 target = targetPosition - position;
        Vector2 acceleration = target * 10.0f;
        velocity += (acceleration * timeElapsed);

        // step 2: speed is capped relative to distance to target
        float distance = target.Length();
        float speed = velocity.Length();
        float maxSpeed = distance * 10.0f;
        if (speed >= maxSpeed) {
            velocity.Normalize();
            velocity *= maxSpeed;
        }

        // step 3: find vector rejection of velocity and target vector, and dampen it a bit
        Vector2 projection = (Vector2.Dot(velocity, target) / Vector2.Dot(target, target)) * target;
        Vector2 rejection = velocity - projection;
        velocity = projection + (0.8f * rejection);

        // step 4: add velocity to position
        position += (velocity * timeElapsed);

        return (position, velocity);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(appleTexture, position, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

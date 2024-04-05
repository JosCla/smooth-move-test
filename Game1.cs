using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SmoothMoveTest;

public delegate (Vector2, Vector2) PosVelModifier(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed);

public class Game1 : Game
{
    public static readonly float APPLE_SIZE = 50.0f;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Vector2 position;
    private Vector2 velocity;

    private Vector2 targetPosition;

    private Texture2D appleTexture;

    private PosVelModifier currModifier;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        position = new Vector2(APPLE_SIZE / 2.0f);
        velocity = Vector2.Zero;

        targetPosition = new Vector2(APPLE_SIZE);

        currModifier = Modifier0;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        appleTexture = this.Content.Load<Texture2D>("apple");
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _spriteBatch.Draw(
            appleTexture,
            position,
            null,
            Color.White,
            0.0f,
            new Vector2(appleTexture.Width / 2.0f, appleTexture.Height / 2.0f),
            new Vector2(APPLE_SIZE / appleTexture.Width, APPLE_SIZE / appleTexture.Height),
            SpriteEffects.None,
            0.0f
        );
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // changing mode
        if (Keyboard.GetState().IsKeyDown(Keys.D0)) {
            currModifier = Modifier0;
        } else if (Keyboard.GetState().IsKeyDown(Keys.D1)) {
            currModifier = Modifier1;
        } else if (Keyboard.GetState().IsKeyDown(Keys.D2)) {
            currModifier = Modifier2;
        } else if (Keyboard.GetState().IsKeyDown(Keys.D3)) {
            currModifier = Modifier3;
        } else if (Keyboard.GetState().IsKeyDown(Keys.D4)) {
            currModifier = Modifier4;
        }

        // updating target pos
        if (Mouse.GetState().LeftButton == ButtonState.Pressed) {
            Point targetPoint = Mouse.GetState().Position;
            targetPosition = new Vector2(targetPoint.X, targetPoint.Y);
        }

        // moving toward target pos
        float timeElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        (position, velocity) = currModifier(position, velocity, targetPosition, timeElapsed);

        base.Update(gameTime);
    }

    // will just "teleport" your position to the target
    private (Vector2, Vector2) Modifier0(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        return (targetPosition, Vector2.Zero);
    }

    // velocity points to target, and speed is proportional to distance
    private (Vector2, Vector2) Modifier1(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        velocity = (targetPosition - position) * 5.0f;
        position += (velocity * timeElapsed);

        return (position, velocity);
    }

    // acceleration points to target with magnitude proportional to distance
    private (Vector2, Vector2) Modifier2(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        // step 1: acceleration is relative to distance to target
        Vector2 acceleration = (targetPosition - position) * 10.0f;
        velocity += (acceleration * timeElapsed);

        // step 2: add velocity to position
        position += (velocity * timeElapsed);

        return (position, velocity);
    }

    // same as last, but with added speed limit
    private (Vector2, Vector2) Modifier3(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        // step 1: acceleration is relative to distance to target
        Vector2 acceleration = (targetPosition - position) * 10.0f;
        velocity += (acceleration * timeElapsed);

        // step 2: speed is capped relative to distance to target
        float distance = (targetPosition - position).Length();
        float speed = velocity.Length();
        float maxSpeed = distance * 10.0f;
        if (speed >= maxSpeed && speed > 0.0f) {
            velocity.Normalize();
            velocity *= maxSpeed;
        }

        // step 3: add velocity to position
        position += (velocity * timeElapsed);

        return (position, velocity);
    }

    // same as last, but with added target rejection dampening
    private (Vector2, Vector2) Modifier4(Vector2 position, Vector2 velocity, Vector2 targetPosition, float timeElapsed)
    {
        // step 1: acceleration is relative to distance to target
        Vector2 target = targetPosition - position;
        Vector2 acceleration = target * 10.0f;
        velocity += (acceleration * timeElapsed);

        // step 2: speed is capped relative to distance to target
        float distance = target.Length();
        float speed = velocity.Length();
        float maxSpeed = distance * 10.0f;
        if (speed >= maxSpeed && speed > 0.0f) {
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
}

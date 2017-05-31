using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MultiShooterGame.GameObjects;
using MonoGameLibrary.Drawing;
using Microsoft.Xna.Framework.Audio;

namespace MultiShooterGame.GameObjects
{
    class Tank : Sprite
    {
        public enum ControlScheme { Empty, MouseKeyboard, Keyboard, XboxControllerOne, XboxControllerTwo, XboxControllerThree, XboxControllerFour }
        public static bool IsXboxControllerScheme(ControlScheme controlScheme)
        {
            return controlScheme == ControlScheme.XboxControllerOne ||
                controlScheme == ControlScheme.XboxControllerTwo ||
                controlScheme == ControlScheme.XboxControllerThree ||
                controlScheme == ControlScheme.XboxControllerFour;
        }

        public static Color ConvertTeamToColor(int team)
        {
            switch (team)
            {
                case 0: return Color.Blue;
                case 1: return Color.FromNonPremultiplied(156, 128, 87, 255);
                case 2: return Color.Red;
                case 3: return Color.Yellow;
                case 4: return Color.Purple;
                case 5: return Color.FromNonPremultiplied(87, 156, 130, 255);
                case 6: return Color.Pink;
                case 7: return Color.Orange;
                case 8: return Color.FromNonPremultiplied(0, 107, 107, 255);
                case 9: return Color.White;
            }
            Console.WriteLine("Unknown Color!");
            return Color.Magenta;
        }
        private int _playerIndex;
        public int PlayerIndex { get { return _playerIndex; } }
        private PlayerIndex _gamePadIndex;
        public PlayerIndex GamePadIndex { get { return _gamePadIndex; } }
        private ControlScheme _usedControlScheme;
        private int _team;
        public int Team { get { return _team; } }
        public Camera2D camera = new Camera2D();
        public void ChangeScale(float newScale)
        {
            cannon.SetScale(newScale);
            SetScale(newScale);
        }

        private Vector2 _cannonOrientation = new Vector2();
        private bool _isCannonLocked = true;
        private bool _useRightStick = false;

        //Stats
        private HealthBar _healthBar;
        private const int cStartHealth = 10;
        private int _health;
        public int Health { get { return _health; } }
        public void ResetHealth() { _health = cStartHealth; }
        public Vector2 statsBarPosition;
        private Sprite tankSprite;
        private Sprite powerupRectangle;
        public bool showHealthStatus = true;

        //Movement
        private float _movementSpeed = 3f;
        private float _velocity;
        private float _oldVelocity;
        private const float acceleration = 0.08f;
        public float Rotation
        {
            set
            {
                rotation = value;
                cannon.rotation = value;
                _orientation = GeometricHelper.GetVectorDirectionFromAngle(rotation);
            }
        }

        //Weapon fields
        private float _timeCharged = 0;
        private const float cFullyChargeTime = 1200;
        private float _coolDownTimer = 0;
        private float coolDownTime = 300;
        private int _chargeLevel = 1;
        private bool _hasBigAmmo = false;
        private bool _hasFastAttack = false;

        //Animation
        private int _animationSpeed = 3;
        private Dictionary<InputState.StickPosition, string> animationMap = new Dictionary<InputState.StickPosition, string>();
        private string _currentAnimationState = "";
        private Vector2 _orientation;
        private Rectangle _screenBounds;
        private Vector2 _movementVector;
        public bool canAct = true;
        private Sprite cannon;

        private Vector2 _receivedForceDirection;
        private float _recievedForceTimer = 200;
        private float _recievedForceTotalTime = 300;
        private float _recievedForceMagnitude = 0;

        private float _vibrationTimer = 0;
        private float _vibrationTotalTime = 200;
        private bool _isVibrating = false;

        private SoundEffectInstance engineSound;

        //Powerups
        private float _powerupTimer;
        private Powerup powerUpInSlot;
        public bool HasRoomForPowerup { get { return powerUpInSlot == null; } }
        private SpriteText _timerText;

        public Tank(int playerIndex, Vector2 position, int team, ControlScheme controlScheme, Rectangle screenBounds)
            : base("Sprites", position, Map.TileSize, Map.TileSize, true, SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.Middle))
        {
            _playerIndex = playerIndex;
            _team = team;
            _screenBounds = screenBounds;
            _usedControlScheme = controlScheme;
            if ((int)_usedControlScheme > 2)
            {
                _gamePadIndex = (PlayerIndex)_usedControlScheme - 3;
            }

            //Cannon
            SetOrigin(16, 16);
            cannon = new Sprite("Sprites", position, 26, 16, false, SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.Middle) + 0.1f);
            cannon.SetTextureRectangle(new Rectangle(96 + (32 * _team), 64, 26, 16));
            cannon.SetOrigin(8f, 8f);
            cannon.position = position;
            _spriteOverlays.Add(cannon);

            //Animation
            AddAnimationState(new SpriteState("MovingHorizontal", SpriteHelper.GetSpriteRectangleStrip(32, 32, 0, 3 + _team, 3 + _team, 0, 2), _animationSpeed));
            _orientation = new Vector2(1, 0);
            _currentAnimationState = "MovingHorizontal";
            SetCurrentAnimationState(_currentAnimationState);
            InitiateAnimationStates();

            //Statbar
            statsBarPosition = new Vector2(20 + (playerIndex * 290), 24);
            if (playerIndex > 1)
            {
                statsBarPosition.X += 200;
            }
            tankSprite = new Sprite("Sprites", (int)statsBarPosition.X, (int)statsBarPosition.Y, new Rectangle(96 + (32 * team), 112, 32, 32), 1);
            _healthBar = new HealthBar((int)statsBarPosition.X + 42, (int)statsBarPosition.Y, 116, 30, Color.Green, Color.Red, Color.Black);
            powerupRectangle = new Sprite("Sprites", _healthBar.x + 100 + 32, 21, 1);
            powerupRectangle.SetTextureRectangle(new Rectangle(0, 176, 36, 36));
        }
        public override void LoadContent(ContentManager contentManager, SpriteBatch spriteBatch)
        {
            base.LoadContent(contentManager, spriteBatch);
            tankSprite.LoadContent(contentManager, spriteBatch);
            powerupRectangle.LoadContent(contentManager, spriteBatch);
            _timerText = new SpriteText("HealthFont", "4", statsBarPosition);
            _timerText.LoadContent(contentManager, spriteBatch);
            engineSound = AudioManager.GetSFXInstance("Engine");
            engineSound.Volume = 0.3f;
            engineSound.IsLooped = true;
            engineSound.Play();
        }

        #region Change State
        public void Reset()
        {
            _health = cStartHealth;
            _healthBar.Percent = 100;
            _chargeLevel = 1;
            _timeCharged = 0;
            _powerupTimer = 0;
            _hasFastAttack = false;
            _hasBigAmmo = false;
            _movementSpeed = 3f;
            coolDownTime = 300;
            powerUpInSlot = null;
            _currentAnimationState = "MovingHorizontal";
            _orientation = new Vector2(1, 0);
            SetCurrentAnimationState(_currentAnimationState);
            canAct = true;
            Show();
            engineSound.Play();
            engineSound.IsLooped = true;
        }
        private void UpdatePowerupTimer(GameTime gameTime)
        {
            if (powerUpInSlot != null && _powerupTimer > 0)
            {
                _timerText.CenterText(new Rectangle((int)powerUpInSlot.position.X, (int)powerUpInSlot.position.Y, 32, 32), true, true);
                _powerupTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (_powerupTimer <= 0)
                {
                    switch (powerUpInSlot.Type)
                    {
                        case Powerup.PowerupType.Speed: _movementSpeed = 3f; break;
                        case Powerup.PowerupType.BigAmmo:
                            _hasBigAmmo = false;
                            if (_chargeLevel == 1)
                            {
                                _scale = 1f;
                                cannon.SetScale(1f);
                            }

                            break;
                        case Powerup.PowerupType.AttackSpeed:
                            _hasFastAttack = false;
                            coolDownTime = 300;
                            break;
                    }
                    powerUpInSlot = null;
                }
            }

        }
        public void ApplyForce(float magnitude, Vector2 direction)
        {
            _receivedForceDirection = direction;
            _recievedForceTimer = 0;
            _recievedForceMagnitude = magnitude;
        }
        public void Damage(Bullet bullet)
        {
            _health -= bullet.damage;
            _healthBar.ChangePercentage(((float)_health / cStartHealth) * 100);
            if (_health <= 0)
            {
                _health = 0;
                canAct = false;
                PooledObjects.explosions.Find(x => !x.IsVisible).Explode(this);
                //Play sound or something
                if (IsXboxControllerScheme(_usedControlScheme) && _isVibrating)
                {
                    GamePad.SetVibration(_gamePadIndex, 0, 0);
                }
                Hide();
            }
            else
            {
                if(bullet.damage==3)
                {
                    AudioManager.PlaySFX("CriticalHit", 1);
                }
                else
                {
                    AudioManager.PlaySFX("Hit" + PlayScreen.random.Next(1, 4), 0.5f);
                }

                _receivedForceDirection = bullet.directionVector;
                _recievedForceTimer = 0;
                _recievedForceMagnitude = bullet.damage * 3f;
                if (IsXboxControllerScheme(_usedControlScheme))
                {
                    _isVibrating = true;
                    _vibrationTimer = 0;
                }
            }

        }

        public void GetPowerup(Powerup powerup)
        {
            AudioManager.PlaySFX("PickUp", 1);
            powerUpInSlot = powerup;
            powerUpInSlot.position = powerupRectangle.position;
            powerUpInSlot.position.X += 2;
            powerUpInSlot.position.Y += 2;
        }
        public void ActivatePowerup()
        {
            AudioManager.PlaySFX("ActivatePowerup", 1);
            switch (powerUpInSlot.Type)
            {
                case Powerup.PowerupType.Speed:
                    _movementSpeed = 5f;
                    break;
                case Powerup.PowerupType.AttackSpeed:
                    _hasFastAttack = true;
                    coolDownTime = 200f;
                    break;
                case Powerup.PowerupType.BigAmmo:
                    _hasBigAmmo = true;
                    _scale = 1.2f;
                    cannon.SetScale(1.2f);
                    break;
                case Powerup.PowerupType.Mines:
                    break;
            }
            //activePowerup = powerUpInSlot;
            //activePowerup.position = powerUpInSlot.position;
            //activePowerup.position.X+= 32 + 16;
            //powerUpInSlot = null;
            _powerupTimer = 5000;
        }
        #endregion

        #region Updating
        public override void Update(GameTime gameTime)
        {
            if (_health > 0)
            {
                if (PlayScreen.usingSplitScreen)
                {
                    float cameraX = position.X - (_screenBounds.Width / 2) + (Map.TileSize / 2);
                    float cameraY = position.Y - (_screenBounds.Height / 2) + (Map.TileSize / 2);
                    camera.Position = new Vector2(cameraX, cameraY);
                }

                if (_recievedForceTimer < _recievedForceTotalTime)
                {
                    _recievedForceTimer += gameTime.ElapsedGameTime.Milliseconds;
                    position += _receivedForceDirection * _recievedForceMagnitude;
                    _recievedForceMagnitude /= 2;
                    AdjustPositionToBounds();
                }
                if (_isVibrating)
                {
                    float vibratingLevel = (_vibrationTotalTime - _vibrationTimer) / _vibrationTotalTime;
                    GamePad.SetVibration(_gamePadIndex, vibratingLevel, vibratingLevel);
                    if (vibratingLevel < 0)
                    {
                        GamePad.SetVibration(_gamePadIndex, 0, 0);
                        _isVibrating = false;
                        _vibrationTimer = 0;
                    }
                    else
                    {
                        _vibrationTimer += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
                UpdatePowerupTimer(gameTime);
            }
        }
        public void HandleWallCollisions(Map currentMap)
        {
            if (_health < 0)
            {
                return;
            }
            int tileX = (int)Math.Round((position.X - Origin.X) / Map.TileSize);
            int tileY = (int)Math.Round((position.Y - Origin.Y) / Map.TileSize);

            Tile left = tileX - 1 >= 0 ? currentMap.tiles[tileX - 1, tileY] : null;
            Tile right = tileX + 1 < currentMap.Width ? currentMap.tiles[tileX + 1, tileY] : null;
            Tile up = tileY - 1 >= 0 ? currentMap.tiles[tileX, tileY - 1] : null;
            Tile down = tileY + 1 < currentMap.Height ? currentMap.tiles[tileX, tileY + 1] : null;

            float diff = 0;
            bool collisionWasSeen = false;
            if (left != null && left.Type == Tile.BlockType.Solid)
            {
                diff = position.X - left.Center.X;
                if (diff < 32)
                {
                    position.X = left.Center.X + 16 + Origin.X;
                    collisionWasSeen = true;
                }
            }
            if (right != null && right.Type == Tile.BlockType.Solid)
            {
                diff = right.Center.X - position.X;
                if (diff < 32)
                {
                    position.X = right.Center.X - (16 + Origin.X);
                    collisionWasSeen = true;
                }
            }
            if (up != null && up.Type == Tile.BlockType.Solid)
            {
                diff = position.Y - up.Center.Y;
                if (diff < 32)
                {
                    position.Y = up.Center.Y + 16 + Origin.Y;
                    collisionWasSeen = true;
                }
            }
            if (down != null && down.Type == Tile.BlockType.Solid)
            {
                diff = down.Center.Y - position.Y;
                if (diff < 32)
                {
                    position.Y = down.Center.Y - (16 + Origin.Y);
                    collisionWasSeen = true;
                }
            }
            if (collisionWasSeen)
            {
                _recievedForceTimer = 0;
            }
        }

        public void AdjustPositionToBounds()
        {
            if (position.X < 48)
            {
                position.X = 48;
            }
            if (position.X > (PlayScreen.MapWidth * Map.TileSize) - 48)
            {
                position.X = (PlayScreen.MapWidth * Map.TileSize) - 48;
            }
            if (position.Y < 48)
            {
                position.Y = 48;
            }
            if (position.Y > (PlayScreen.MapHeight * Map.TileSize) - 48)
            {
                position.Y = (PlayScreen.MapHeight * Map.TileSize) - 48;
            }
        }
        public void HandleInput(InputState inputState, GameTime gameTime)
        {
            if (canAct)
            {
                if (_health > 0)
                {
                    if (IsXboxControllerScheme(_usedControlScheme))
                    {
                        inputState.ActivePlayerIndex = _gamePadIndex;
                    }
                    HandleMovement(inputState, gameTime);
                    HandleOrientation(inputState);
                    HandleShooting(inputState, gameTime);

                    //Powerup
                    if (powerUpInSlot != null && _powerupTimer <= 0 &&
                        (inputState.IsButtonNewPressed(Buttons.Y) ||
                        inputState.IsKeyNewPressed(Keys.LeftShift)))
                    {
                        ActivatePowerup();
                    }
                }

            }
        }
        private void HandleMovement(InputState inputState, GameTime gameTime)
        {
            Vector2 previousMovementVector = _movementVector;
            _movementVector = Vector2.Zero;
            if (IsXboxControllerScheme(_usedControlScheme))
            {
                _movementVector = inputState.GetLeftStickPosition();
                if (_movementVector != Vector2.Zero)
                {
                    //Dualstick
                    _movementVector.Y *= -1;
                }
                else
                {
                    if (inputState.IsButtonPressed(Buttons.DPadLeft)) { _movementVector.X = -1.0f; }
                    else if (inputState.IsButtonPressed(Buttons.DPadRight)) { _movementVector.X = 1.0f; }
                    if (inputState.IsButtonPressed(Buttons.DPadUp)) { _movementVector.Y = -1.0f; }
                    else if (inputState.IsButtonPressed(Buttons.DPadDown)) { _movementVector.Y = 1.0f; }
                }
            }
            else if (_usedControlScheme == ControlScheme.Keyboard)
            {
                if (inputState.IsKeyPressed(Keys.Left)) { _movementVector.X = -1.0f; }
                else if (inputState.IsKeyPressed(Keys.Right)) { _movementVector.X = 1.0f; }
                if (inputState.IsKeyPressed(Keys.Up)) { _movementVector.Y = -1.0f; }
                else if (inputState.IsKeyPressed(Keys.Down)) { _movementVector.Y = 1.0f; }
            }
            if (_movementVector != Vector2.Zero)
            {
                _orientation = _movementVector;
            }
            _oldVelocity = _velocity;
            if (_movementVector != Vector2.Zero)
            {
                _velocity = Math.Min(_oldVelocity + (acceleration), _movementSpeed / _chargeLevel);
            }
            else
            {
                _velocity = Math.Max(_oldVelocity - acceleration, 0);
            }
            position += _movementVector * ((_oldVelocity + _velocity) / 2);
            float newPitch =  (_velocity / _movementSpeed);
            if(newPitch>=0 && newPitch<=1)
            {
                engineSound.Pitch =newPitch;
            }
        }
        private void HandleOrientation(InputState inputState)
        {

            if (_orientation.X != 0 || _orientation.Y != 0)
            {
                _orientation.Normalize();
                rotation = GeometricHelper.GetAngleFromVectorDirection(_orientation);
            }
            InputState.StickPosition currentPosition = InputState.ConvertVectorDirectionToStickPosition(_orientation);
            string newAnimation = "";
            if (animationMap.ContainsKey(currentPosition))
            {
                newAnimation = animationMap[currentPosition];
            }

            if (!newAnimation.Equals(_currentAnimationState))
            {
                _currentAnimationState = newAnimation;
                SetCurrentAnimationState(_currentAnimationState);
            }
            if (_movementVector.X == 0 && _movementVector.Y == 0)
            {
                PauseAnimation();
            }
            else
            {
                ResumeAnimation();
            }
        }
        private void HandleShooting(InputState inputState, GameTime gameTime)
        {
            _coolDownTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (_hasFastAttack && _coolDownTimer <= 0)
            {
                Shoot();
                return;
            }
            bool isShooting = false;
            bool isCharging = false;
            switch (_usedControlScheme)
            {
                case ControlScheme.Empty: break;
                case ControlScheme.Keyboard:
                    isShooting = inputState.IsKeyNewReleased(Keys.Space);
                    isCharging = inputState.IsKeyPressed(Keys.Space);
                    break;
                default://Xbox controller
                    if (!_useRightStick)
                    {
                        isShooting = inputState.IsButtonNewReleased(Buttons.X);
                        isCharging = inputState.IsButtonPressed(Buttons.X);
                    }
                    else
                    {
                        isShooting = inputState.IsButtonNewReleased(Buttons.RightTrigger);
                        isCharging = inputState.IsButtonPressed(Buttons.RightTrigger);
                    }

                    break;
            }

            if (isCharging && _timeCharged < cFullyChargeTime)
            {
                _timeCharged = Math.Min(_timeCharged + gameTime.ElapsedGameTime.Milliseconds, cFullyChargeTime);
                if (_timeCharged >= cFullyChargeTime || (_timeCharged >= cFullyChargeTime / 2 && _hasBigAmmo))
                {
                    _scale = 1.4f;
                    cannon.SetScale(1.4f);
                    _chargeLevel = 3;
                }
                else if (_timeCharged >= cFullyChargeTime / 2)
                {
                    _scale = 1.2f;
                    cannon.SetScale(1.2f);
                    _chargeLevel = 2;
                }
            }
            else
            {

                if (isShooting && _coolDownTimer <= 0)
                {
                    Shoot();
                }
            }

        }
        private void Shoot()
        {
            Vector2 bulletPosition = position + Origin;//Add some offset here
            Vector2 direction = _useRightStick ? _cannonOrientation : _orientation;
            PooledObjects.bullets.Find(b => !b.IsVisible).Activate(position, direction, 10, _hasBigAmmo ? 2 : _chargeLevel, _team);
            _timeCharged = 0;
            _coolDownTimer = coolDownTime;

            _chargeLevel = 1;
            if (_hasBigAmmo)
            {
                _scale = 1.2f;
                cannon.SetScale(1.2f);
            }
            else
            {
                _scale = 1f;
                cannon.SetScale(1f);
            }

            ApplyForce(_chargeLevel * _chargeLevel * _chargeLevel, -_cannonOrientation);
        }
        #endregion

        public override void Draw(GameTime gameTime)
        {
            cannon.position = position;
            if (!_useRightStick)
            {
                cannon.rotation = rotation;
            }
            base.Draw(gameTime);
        }
        public void DrawStatsBar(GameTime gameTime)
        {
            if (showHealthStatus)
            {
                tankSprite.Draw(gameTime);

                _healthBar.Draw(_spriteBatch, gameTime);

                powerupRectangle.Draw(gameTime);
                if (powerUpInSlot != null)
                {
                    powerUpInSlot.Draw(gameTime);
                    if (_powerupTimer > 0)
                    {
                        _timerText.text = (Math.Round(_powerupTimer / 1000, 0)).ToString();
                        _timerText.Draw(gameTime);
                    }
                }

            }
        }

    }
}

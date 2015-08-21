using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace ShooterGuys
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
            switch(team)
            {
                case 0: return Color.Blue;
                case 1: return Color.Brown;
                case 2:return Color.Red;
                case 3: return Color.Yellow;
            }
            Console.WriteLine("Unknown Color!");
            return Color.Magenta;
        }

		private PlayerIndex _gamePadIndex;
		public PlayerIndex GamePadIndex { get { return _gamePadIndex; } }
		private ControlScheme _usedControlScheme;
		public Camera2D camera = new Camera2D();

		//Stats
		private const int cStartHealth = 100;
		private int _health;
		public int Health { get { return _health; } }
		private const float cMovementSpeed = 3f;

		//Weapon fields
		private float _timeCharged = 0;
		private const float cFullyChargeTime = 1000;
		private float _coolDownTimer = 0;
		private const float cCoolDownTime = 300;

		//Animation
        private int _animationSpeed = 3;
		private Dictionary<InputState.StickPosition, string> animationMap = new Dictionary<InputState.StickPosition, string>();
		private string _currentAnimationState = "";
		private Vector2 _orientation;
		private Rectangle _screenBounds;
		private Vector2 _addedMovement;
        public bool canAct = true;

        public Sprite cannon;

		private Vector2 _receivedForceDirection;
		private float _recievedForceTimer = 200;
		private float _recievedForceTotalTime = 300;
		private float _recievedForceMagnitude = 0;

		private float _vibrationTimer = 0;
		private float _vibrationTotalTime = 200;
		private bool _isVibrating = false;

        private int _team;
        public int Team { get { return _team; } }

        public Tank(int index, Vector2 position, int team, ControlScheme controlScheme, Rectangle screenBounds)
            : base("Sprites", position, Map.TileSize, Map.TileSize, true, SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.Middle))
		{
            _team = team;
			_screenBounds = screenBounds;
			_usedControlScheme = controlScheme;
            SetOrigin(Map.TileSize / 2, Map.TileSize / 2);
            cannon = new Sprite("Sprites", position, 26, 16, false, SpriteHelper.GetDefaultDepth(SpriteHelper.SpriteDepth.Middle) + 0.1f);
            cannon.SetTextureRectangle(new Rectangle(96 + 32 * index, 64, 26, 16));
            cannon.SetOrigin(8f, 8f);
            cannon.position = position;
            cannon.rotation = GeometricHelper.GetAngleFromVectorDirection(_orientation);
            _spriteOverlays.Add(cannon);
            if ((int)_usedControlScheme > 2)
			{
				_gamePadIndex = (PlayerIndex)_usedControlScheme - 3;
            }
            AddAnimationState(new SpriteState("MovingHorizontal", SpriteHelper.GetSpriteRectangleStrip(32, 32, 0, 3 + index, 3 + index, 0, 2), _animationSpeed));

            _currentAnimationState = "MovingHorizontal";
			_orientation = new Vector2(1, 0);
			SetCurrentAnimationState(_currentAnimationState);            
			InitiateAnimationStates();
			_health = cStartHealth;


		}
		public void Reset()
		{
			_health = cStartHealth;
			_currentAnimationState = "WalkRight";
			_orientation = new Vector2(1, 0);
			SetCurrentAnimationState(_currentAnimationState);
			Show();
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
			if (_health <= 0)
			{
				//Play sound or something
				if (IsXboxControllerScheme(_usedControlScheme) && _isVibrating)
				{
					GamePad.SetVibration(_gamePadIndex, 0, 0);
				}
			}
			else
			{
				_receivedForceDirection = bullet.directionVector;
				_recievedForceTimer = 0;
				_recievedForceMagnitude = bullet.damage / 4;
				if (IsXboxControllerScheme(_usedControlScheme))
				{
					_isVibrating = true;
					_vibrationTimer = 0;
				}
			}

		}
		public override void Update(GameTime gameTime)
		{
			if (_health > 0)
			{
				if (PlayScreen.usingSplitScreen)
				{
					float cameraX = position.X - (_screenBounds.Width / 2) + (Map.TileSize / 2);
					float cameraY = position.Y - (_screenBounds.Height / 2) + (Map.TileSize / 2);
					camera.SetPosition(cameraX, cameraY);
				}

				if (_recievedForceTimer < _recievedForceTotalTime)
				{
					_recievedForceTimer += gameTime.ElapsedGameTime.Milliseconds;
					position += _receivedForceDirection * _recievedForceMagnitude;
				}
				if (_isVibrating)
				{
					float vibratingLevel = (_vibrationTotalTime - _vibrationTimer) / _vibrationTotalTime;
					GamePad.SetVibration(_gamePadIndex, vibratingLevel, vibratingLevel);
					if (vibratingLevel < 0)
					{
						GamePad.SetVibration(_gamePadIndex, 0, 0);
						_isVibrating = false;
					}
					else
					{
						_vibrationTimer += gameTime.ElapsedGameTime.Milliseconds;
					}
				}

            }
		}
		public void HandleWallCollisions(Map currentMap)
		{
			int tileX = (int)Math.Round((position.X-Origin.X) / Map.TileSize);
			int tileY = (int)Math.Round((position.Y-Origin.Y) / Map.TileSize);

			Tile left = tileX - 1 >= 0 ? currentMap.tiles[tileX - 1, tileY] : null;
			Tile right = tileX + 1 < currentMap.Width ? currentMap.tiles[tileX + 1, tileY] : null;
			Tile up = tileY - 1 >= 0 ? currentMap.tiles[tileX, tileY - 1] : null;
			Tile down = tileY + 1 < currentMap.Height ? currentMap.tiles[tileX, tileY + 1] : null;

			float diff = 0;

			if (left != null && left.Type == Tile.BlockType.Solid)
			{
                diff = position.X-left.Center.X;
				if (diff < 32)
				{
                    position.X = left.Center.X + 16+Origin.X;
				}
			}
			if (right != null && right.Type == Tile.BlockType.Solid)
			{
                diff = right.Center.X - position.X;
                if (diff < 32)
				{
                    position.X = right.Center.X - (16 + Origin.X);
				}
			}
			if (up != null && up.Type == Tile.BlockType.Solid)
			{
                diff = position.Y-up.Center.Y;
                if (diff < 32)
				{
                    position.Y = up.Center.Y + 16 + Origin.Y;
				}
			}
			if (down != null && down.Type == Tile.BlockType.Solid)
			{
                diff = down.Center.Y-position.Y;
                if (diff < 32)
				{
                    position.Y = down.Center.Y - (16 + Origin.Y);
				}
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
				    HandleMovement(inputState);
				    HandleOrientation(inputState);
				    HandleShooting(inputState, gameTime);
			    }
            }
		}
		private void HandleMovement(InputState inputState)
		{
			_addedMovement = new Vector2(0, 0);
			if (IsXboxControllerScheme(_usedControlScheme))
			{
				_addedMovement = inputState.GetLeftStickPosition() * cMovementSpeed;
				//Dualstick
				_addedMovement.Y *= -1;
			}
			else if (_usedControlScheme == ControlScheme.Keyboard)
			{
                if(inputState.IsKeyPressed(Keys.Left)){_addedMovement.X=-1.0f*cMovementSpeed;}
                else if(inputState.IsKeyPressed(Keys.Right)){_addedMovement.X=1.0f*cMovementSpeed;}
                if(inputState.IsKeyPressed(Keys.Up)){_addedMovement.Y=-1.0f*cMovementSpeed;}
                else if(inputState.IsKeyPressed(Keys.Down)){_addedMovement.Y=1.0f*cMovementSpeed;}

                if (_addedMovement.X!=0 || _addedMovement.Y != 0 )
                {
                    rotation = GeometricHelper.GetAngleFromVectorDirection(_addedMovement);
                }
                
			}
			else if (_usedControlScheme == ControlScheme.MouseKeyboard)
			{
				_addedMovement.X += (inputState.IsKeyPressed(Keys.A) ? -1 : 0) * cMovementSpeed;
				_addedMovement.X += (inputState.IsKeyPressed(Keys.D) ? 1 : 0) * cMovementSpeed;
				_addedMovement.Y += (inputState.IsKeyPressed(Keys.W) ? -1 : 0) * cMovementSpeed;
				_addedMovement.Y += (inputState.IsKeyPressed(Keys.S) ? 1 : 0) * cMovementSpeed;
                if (_addedMovement.X != 0 || _addedMovement.Y != 0)
                {
                    rotation = GeometricHelper.GetAngleFromVectorDirection(_addedMovement);
                }
			}
			
			position += _addedMovement;
		}

		private void HandleOrientation(InputState inputState)
		{
			switch (_usedControlScheme)
			{
				case ControlScheme.Empty: break;
				case ControlScheme.Keyboard:
					if (_addedMovement.X != 0 || _addedMovement.Y != 0)
					{
						_orientation = _addedMovement;
						_orientation.Normalize();
					}
					break;
				default://Xbox controller
					if (_addedMovement.X != 0 || _addedMovement.Y != 0)
					{
						_orientation = inputState.GetLeftStickPosition();
						_orientation.Y *= -1;
						_orientation.Normalize();
                        rotation = GeometricHelper.GetAngleFromVectorDirection(_orientation);
					}
					break;
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
            if (_addedMovement.X == 0 && _addedMovement.Y == 0)
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
			bool isShooting = false;
			bool isCharging = false;
			switch (_usedControlScheme)
			{
				case ControlScheme.Empty: break;
				case ControlScheme.Keyboard:
					isShooting = inputState.IsKeyNewReleased(Keys.Space);
					isCharging = inputState.IsKeyPressed(Keys.Space);
					break;
				case ControlScheme.MouseKeyboard:
					isShooting = inputState.IsMouseLeftButtonNewReleased();
					isCharging = inputState.IsMouseLeftButtonPressed();
					break;
				default://Xbox controller
					isShooting = inputState.IsButtonNewReleased(Buttons.X);
					isCharging = inputState.IsButtonPressed(Buttons.X);
					break;
			}
			_coolDownTimer -= gameTime.ElapsedGameTime.Milliseconds;
			if (isCharging)
			{
				_timeCharged = Math.Min(_timeCharged + gameTime.ElapsedGameTime.Milliseconds, cFullyChargeTime);
			}
			else
			{

				if (isShooting && _coolDownTimer <= 0)
				{
					Vector2 bulletPosition = position+Origin;//Add some offset here

					float sizeOfBullet = (Math.Max(1000, _timeCharged) / cFullyChargeTime) * 3;
					PooledObjects.bullets.Find(b => !b.isVisible).Activate(position, _orientation, 10, sizeOfBullet, _team);
					_timeCharged = 0;
					_coolDownTimer = cCoolDownTime;
				}
			}

		}

        public override void Draw(GameTime gameTime)
        {
            cannon.position = position;
            cannon.rotation = GeometricHelper.GetAngleFromVectorDirection(_orientation);
            base.Draw(gameTime);
        }

    }
}

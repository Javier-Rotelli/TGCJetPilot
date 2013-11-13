using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils.Input;

namespace AlumnoEjemplos.Jet_Pilot
{
	/// <summary>
	/// Camara que permite rotar y hacer zoom alrededor de un objeto central
	/// </summary>
	public class FreeCam : TgcCamera
	{
		public static float DEAFULT_SPEED = 10.0f;

		bool enable;

		float speed;

		Vector3 up;
		Vector3 center;
		Vector3 target;

		Vector3 nextCenter;
		Vector3 nextTarget;
		Vector3 nextUp;

		Matrix viewMatrix;

		public FreeCam()
		{
			resetValues();
		}

		public void SetCenterTargetUp(Vector3 _center, Vector3 _target, Vector3 _up)
		{
			SetCenterTargetUp(_center, _target, _up, false);
		}

		public void SetCenterTargetUp(Vector3 _center, Vector3 _target, Vector3 _up, bool teleport)
		{
			nextCenter = _center;
			nextTarget = _target;
			nextUp = _up;

			if (teleport)
			{
				center = _center;
				target = _target;
				up = _up;

				viewMatrix = Matrix.LookAtLH(center, target, up);
			}
		}

		public bool Enable
		{
			get { return enable; }
			set
			{
				enable = value;
				//Si se habilito la cámara, cargar como la cámara actual
				if (value)
				{
					GuiController.Instance.CurrentCamera = this;
				}
			}
		}

		/// <summary>
		/// Carga los valores default de la camara
		/// </summary>
		internal void resetValues()
		{
			up = new Vector3(0.0f, 1.0f, 0.0f);
			center = new Vector3(0, 0, 0);
			target = new Vector3(0, 0, -1.0f);

			nextCenter = center;
			nextTarget = target;
			nextUp = up;

			speed = DEAFULT_SPEED;

			viewMatrix = Matrix.Identity;
		}

		/// <summary>
		/// Actualiza los valores de la camara
		/// </summary>
		public void updateCamera()
		{
			if (!enable)
			{
				return;
			}

            float delta = GuiController.Instance.ElapsedTime * speed;
			if (delta > 1.0f) delta = 1.0f;

			center += (nextCenter - center) * delta;
			target += (nextTarget - target) * delta;
			up += (nextUp - up) * delta;

			viewMatrix = Matrix.LookAtLH(center, target, up);
		}

		/// <summary>
		/// Actualiza la ViewMatrix, si es que la camara esta activada
		/// </summary>
		public void updateViewMatrix(Microsoft.DirectX.Direct3D.Device d3dDevice)
		{
			if (!enable)
			{
				return;
			}

			d3dDevice.Transform.View = viewMatrix;
		}


		public Vector3 getPosition()
		{
			return center;
		}

		public Vector3 getLookAt()
		{
			return target;
		}
	}
}
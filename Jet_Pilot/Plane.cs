using System;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Jet_Pilot
{
	class Plane
	{
		TgcMesh plane;
		float pitchSpeed;
		float rollSpeed;
		float velocidad_aceleracion;
		int pitch;
		int roll;
		int acelerador;

		float velocidad_tangente;
		float velocidad_normal;

		Vector3 deathPos;
		bool dead;

		public Plane()
		{
			String path = GuiController.Instance.ExamplesMediaDir + @"MeshCreator\Meshes\Vehiculos\AvionCaza\AvionCaza-TgcScene.xml";
			TgcSceneLoader loader = new TgcSceneLoader();
			plane = loader.loadSceneFromFile(path).Meshes[0];
			plane.AutoTransformEnable = false;

			rollSpeed = 2.5f;
			pitchSpeed = 2.0f;
			velocidad_normal = 0.6f;
			velocidad_aceleracion = 150.0f;

			Reset();
		}

        //hago el getter del avion
        public TgcMesh getMesh()
        {
            return this.plane;
        }

		public void Reset()
		{
			pitch = 0;
			roll = 0;
			acelerador = 0;

			velocidad_tangente = 250.0f;

			Matrix m = plane.Transform;
			m = Matrix.Identity;
			plane.Transform = m;
			SetPosition(new Vector3 (0, 1500.0f, 0));

			dead = false;
		}

		public void Update(float dt)
		{
			if (dead) return;

			// Mover el avion hacia donde esta apuntando
			Vector3 dir = -ZAxis();
			Translate(dir * velocidad_tangente * dt);

			// Rotar el avion de acuerdo a los comandos recibidos
			if (pitch != 0)
			{
				RotateAroundX(pitch * pitchSpeed * dt);
			}
			if (roll != 0)
			{
				RotateAroundZ(roll * rollSpeed * dt);
			}

			if (acelerador != 0)
			{
				velocidad_tangente += velocidad_aceleracion * acelerador * dt;
				if (velocidad_tangente < 100.0f) velocidad_tangente = 100.0f;
				if (velocidad_tangente > 500.0f) velocidad_tangente = 500.0f;
			}

			// La rotacion depende de cuanto este inclinado hacia los lados,
			// esto se puede calcular con la componente en Y del eje X del avión
			float turn = plane.Transform.M12 * velocidad_normal * dt;

			// Y rotamos el avion respecto a la vertical que lo atraviesa
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), new Vector3(0, 1, 0), turn);
			m.Multiply(r);
			plane.Transform = m;
		}


		public void SetYoke(bool up, bool down, bool left, bool right)
		{
			pitch = 0;
			roll = 0;
			if (up && !down)
			{
				pitch = -1;
			}
			else if (down && !up)
			{
				pitch = 1;
			}
			if (left && !right)
			{
				roll = -1;
			}
			else if (right && !left)
			{
				roll = 1;
			}
		}

		public void SetThrottle(bool plus, bool minus)
		{
			acelerador = 0;
			if (plus && !minus)
			{
				acelerador = 1;
			}
			else if (!plus && minus)
			{
				acelerador = -1;
			}
		}

		public void SetPitchSpeed(float s)
		{
			pitchSpeed = s;
		}

		public void SetRollSpeed(float s)
		{
			rollSpeed = s;
		}

		public void SetTurnSpeed(float s)
		{
			velocidad_normal = s;
		}

		public void Render()
		{
		    plane.render();
		}

		public float GetAirSpeed()
		{
			return velocidad_tangente;
		}

		public Vector3 XAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M11, m.M12, m.M13);
		}

		public Vector3 YAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M21, m.M22, m.M23);
		}

		public Vector3 ZAxis()
		{
			Matrix m = plane.Transform;
			return new Vector3(m.M31, m.M32, m.M33);
		}

		public void Translate(Vector3 delta)
		{
			Matrix m = plane.Transform;
			Matrix t = Matrix.Identity;
			t.M41 = delta.X;
			t.M42 = delta.Y;
			t.M43 = delta.Z;
			m.Multiply(t);
			plane.Transform = m;
		}

		public Vector3 GetPosition()
		{
		    Matrix m = plane.Transform;
			return new Vector3(m.M41, m.M42, m.M43);
		}

		public void SetPosition(Vector3 p)
		{
			Matrix m = plane.Transform;
			m.M41 = p.X;
			m.M42 = p.Y;
			m.M43 = p.Z;
			plane.Transform = m;
		}

		public void RotateAroundX(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), XAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
		}
		public void RotateAroundY(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), YAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
		}
		public void RotateAroundZ(float ang)
		{
			Matrix m = plane.Transform;
			Matrix r = CreateRotationMatrix(GetPosition(), ZAxis(), ang);
			m.Multiply(r);
			plane.Transform = m;
		}

		Matrix CreateRotationMatrix(Vector3 p, Vector3 dir, float ang)
		{
			dir.Normalize();

			float cos = (float)Math.Cos(ang);
			float sin = (float)Math.Sin(ang);

			Matrix r = Matrix.Identity;
			r.M11 = dir.X * dir.X + (dir.Y * dir.Y + dir.Z * dir.Z) * cos;
			r.M21 = dir.X * dir.Y * (1 - cos) - dir.Z * sin;
			r.M31 = dir.X * dir.Z * (1 - cos) + dir.Y * sin;

			r.M12 = dir.X * dir.Y * (1 - cos) + dir.Z * sin;
			r.M22 = dir.Y * dir.Y + (dir.X * dir.X + dir.Z * dir.Z) * cos;
			r.M32 = dir.Y * dir.Z * (1 - cos) - dir.X * sin;

			r.M13 = dir.X * dir.Z * (1 - cos) - dir.Y * sin;
			r.M23 = dir.Y * dir.Z * (1 - cos) + dir.X * sin;
			r.M33 = dir.Z * dir.Z + (dir.X * dir.X + dir.Y * dir.Y) * cos;

			r.M41 = (p.X * (dir.Y * dir.Y + dir.Z * dir.Z) - dir.X * (p.Y * dir.Y + p.Z * dir.Z)) * (1 - cos) + (p.Y * dir.Z - p.Z * dir.Y) * sin;
			r.M42 = (p.Y * (dir.X * dir.X + dir.Z * dir.Z) - dir.Y * (p.X * dir.X + p.Z * dir.Z)) * (1 - cos) + (p.Z * dir.X - p.X * dir.Z) * sin;
			r.M43 = (p.Z * (dir.X * dir.X + dir.Y * dir.Y) - dir.Z * (p.X * dir.X + p.Y * dir.Y)) * (1 - cos) + (p.X * dir.Y - p.Y * dir.X) * sin;

			return r;
		}
	}
}

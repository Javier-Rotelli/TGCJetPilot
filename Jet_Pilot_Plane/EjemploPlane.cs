using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX.DirectInput;

namespace AlumnoEjemplos.Jet_Pilot
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
		Vector3 CAM_DELTA = new Vector3(0, 50, 250);
		Plane player;
		FreeCam cam;
		bool gameOver;
		bool reset;

		/// <summary>
		/// Categoría a la que pertenece el ejemplo.
		/// Influye en donde se va a haber en el árbol de la izquierda de la pantalla.
		/// </summary>
		public override string getCategory()
		{
			return "AlumnoEjemplos";
		}

		/// <summary>
		/// Completar nombre del grupo en formato Grupo NN
		/// </summary>
		public override string getName()
		{
			return "Jet_Pilot";
		}

		/// <summary>
		/// Completar con la descripción del TP
		/// </summary>
		public override string getDescription()
		{
			return "Simulador de vuelo";
		}

		/// <summary>
		/// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
		/// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
		/// Borrar todo lo que no haga falta
		/// </summary>
		public override void init()
		{
			
			// Crear avion del jugador
			player = new Plane();

			// Crear la cámara
			cam = new FreeCam();
			cam.SetCenterTargetUp(CAM_DELTA, new Vector3(0, 0, 0), new Vector3(0, 1, 0), true);
			cam.Enable = true;
			GuiController.Instance.CurrentCamera = cam;

			GuiController.Instance.Modifiers.addFloat("Velocidad de rotación", 0.5f, 1.0f, 0.6f);
			GuiController.Instance.Modifiers.addFloat("Velocidad de pitch", 1.0f, 3.0f, 2.0f);
			GuiController.Instance.Modifiers.addFloat("Velocidad de roll", 1.0f, 3.0f, 2.5f);

            GuiController.Instance.UserVars.addVar("Posición en X");
            GuiController.Instance.UserVars.addVar("Posición en Y");
            GuiController.Instance.UserVars.addVar("Posición en Z");

            GuiController.Instance.UserVars.addVar("Avión respecto a X");
            GuiController.Instance.UserVars.addVar("Avión respecto a Y");
            GuiController.Instance.UserVars.addVar("Avión respecto a Z");

			Reset();
		}

		void Reset()
		{
			gameOver = false;

			reset = false;

			player.Reset();

			// Extraigo los ejes del avion de su matriz transformación
			Vector3 plane = player.GetPosition();
			Vector3 z = player.ZAxis();
			Vector3 y = player.YAxis();

			// Seteo la cámara en función de la posición del avion
			Vector3 camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
			Vector3 target = plane + CAM_DELTA.Y * y;
			cam.SetCenterTargetUp(camera, target, y, true);

		}

		/// <summary>
		/// Método que se llama cada vez que hay que refrescar la pantalla.
		/// Escribir aquí todo el código referido al renderizado.
		/// Borrar todo lo que no haga falta
		/// </summary>
		/// <param name="elapsedTime">Tiempo en segundos transcurridos desde el último frame</param>
		public override void render(float elapsedTime)
		{
			// Este método registra el input del jugador
			CheckInput(elapsedTime);

			// Acá esta la lógica de juego
			Update(elapsedTime);

			// Acá se hace el verdadero render, se dibuja la pantalla
			Draw(elapsedTime);
		}

		/// <summary>
		/// Método que se llama cuando termina la ejecución del ejemplo.
		/// Hacer dispose() de todos los objetos creados.
		/// </summary>
		public override void close()
        {
		}

		/// <summary>
		/// Registra los comandos del jugador y los modifiers
		/// </summary>
		/// <param name="dt">Tiempo desde la última ejecución</param>
		public void CheckInput(float dt)
		{
			if (!gameOver)
			{
				// El menu se activa solo al chocar, y no se puede salir, solo resetear
				bool enter = GuiController.Instance.D3dInput.keyPressed(Microsoft.DirectX.DirectInput.Key.Return);
			}

			bool up = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.UpArrow);
			bool down = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.DownArrow);
			bool left = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftArrow);
			bool right = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.RightArrow);

			bool plus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Q);
			bool minus = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.A);

			player.SetYoke(up, down, left, right);
			player.SetThrottle(plus, minus);

			bool shoot = GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.Space);

			if (shoot)
			{
				Vector3 posIni = player.GetPosition();
				Vector3 z = -player.ZAxis();
			}

	

			// Check modifiers
			float turnSpeed = (float)GuiController.Instance.Modifiers["Velocidad de rotación"];
			float pitchSpeed = (float)GuiController.Instance.Modifiers["Velocidad de pitch"];
			float rollSpeed = (float)GuiController.Instance.Modifiers["Velocidad de roll"];
			player.SetTurnSpeed(turnSpeed);
			player.SetPitchSpeed(pitchSpeed);
			player.SetRollSpeed(rollSpeed);

		}

		/// <summary>
		/// Lógica del juego. Actualiza posiciones, estados, etc.
		/// </summary>
		/// <param name="dt">Tiempo desde la última ejecución</param>
		public void Update(float dt)
		{
			// Comando de resetear el juego
			if (reset)
			{
				Reset();
				return;
			}

			// Control de saltos en casos de bajo rendimiento y pérdida del foco
			if (dt > 0.1f) dt = 0.1f;

			player.Update(dt);

			// Extraigo los ejes del avion de su matriz transformación
			Vector3 plane = player.GetPosition();
			Vector3 z = player.ZAxis();
			Vector3 y = player.YAxis();
            Vector3 x = player.XAxis();

            GuiController.Instance.UserVars.setValue("Posición en X",plane.X);
            GuiController.Instance.UserVars.setValue("Posición en Y", plane.Y);
            GuiController.Instance.UserVars.setValue("Posición en Z", plane.Z);

            GuiController.Instance.UserVars.setValue("Avión respecto a X", x);
            GuiController.Instance.UserVars.setValue("Avión respecto a Y", y);
            GuiController.Instance.UserVars.setValue("Avión respecto a Z", z);

			Vector3 camera;
			Vector3 target;
			
			camera = plane + CAM_DELTA.Y * y + CAM_DELTA.Z * z;
			target = plane + CAM_DELTA.Y * y;
			cam.SetCenterTargetUp(camera, target, y);
			
		}

		/// <summary>
		/// Dibuja en la pantalla
		/// </summary>
		/// <param name="dt">Tiempo desde la última ejecución</param>
		public void Draw(float dt)
		{
			player.Render();
		}
	}
}

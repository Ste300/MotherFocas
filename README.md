# MotherFocas
Proyecto multijugador de focas

Implementación del Menú Principal
Se creó un menú principal funcional dentro del proyecto, el cual incluye:
•	Un panel principal con opciones básicas del juego.

Sistema de Turnos y Rondas
Dentro de la lógica del TeamManager se desarrolló:
•	Alternancia automática entre Equipo 1 y Equipo 2.
•	Detección de cuándo una foca termina su acción para permitir continuar el turno.
•	Reinicio automático de rondas cuando ambos equipos utilizan todas sus focas disponibles.
•	Métodos para reiniciar las posiciones y estados iniciales de cada unidad.
•	Control de disponibilidad de focas para evitar acciones repetidas por ronda.
•	Mensajería interna para mostrar en UI qué ronda está activa y qué equipo tiene el turno.

Reseteo Completo de la Partida
Tras concluir una partida o al usar el botón Salir del menú de pausa:
•	Se limpian las posiciones de todas las focas.
•	Se restauran su vida, rotación, estado físico y disponibilidad.
•	Los GameObjects desactivados vuelven a activarse.
•	Se garantiza que el menú principal aparece y el panel de pausa desaparece correctamente.
Sistema de Pausa
Se implementó un panel de pausa que puede abrirse de dos formas:
•	Presionando la tecla ESC.
Interfaz del Juego (HUD)
Se incluyó un HUD que muestra:
•	La ronda actual.
•	El equipo que tiene el turno.
•	La información aparece mediante campos de texto actualizables en el Inspector.
integración con Photon (Multijugador)
Se configuró el sistema multijugador básico utilizando Photon, estableciendo:
•	Un jugador que actúa como host 
•	Un jugador que funciona como cliente 



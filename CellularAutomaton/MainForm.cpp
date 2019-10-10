#include "MainForm.h"
#include <array>

using namespace System;
using namespace System::Windows::Forms;


const int SpaceSize = 300;
int GrainsNumber = 0;
std::array<std::array<int, SpaceSize>, SpaceSize> previousStep;
std::array<std::array<int, SpaceSize>, SpaceSize> nextStep;


class Cell {
public:
	int grainID;

	Cell() {
		grainID = -1;
	}

	void AddToGrain(int id) {
		grainID = id;
	}
};

class Grain {
public:
	int ID;

	Grain() {
		ID = GrainsNumber;
		GrainsNumber++;
	}
};

void InitializeAutomaton(CellularAutomaton::MainForm form) {
	form.pictureBox1->Image = Image::F
}


[STAThread]
void Main(array<String^>^ args)
{
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false);

	CellularAutomaton::MainForm form;
	Application::Run(%form);
	InitializeAutomaton(&form);
}

	
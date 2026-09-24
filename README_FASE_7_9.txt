EVOLUTIONSIM — FASE 7.9
FEEDINGSPECIALIZATION — PRIMER EFECTO ACTIVO
=============================================

OBJETIVO
--------
Activar experimentalmente el quinto canal fenotípico original:

FeedingSpecialization

mediante una sola vía causal.

ESTADO DE LOS CANALES
---------------------
MetabolicEfficiency
- activo
- coupling: 0.125
- efecto máximo: ±1 %

ThermalRegulation
- activo
- coupling: 0.125
- efecto máximo: ±1 %

BodySize
- pasivo
- coupling normal: 0.0

Locomotion
- pasivo
- coupling normal: 0.0

FeedingSpecialization
- experimental
- coupling normal: 0.0

UNA SOLA VÍA CAUSAL
-------------------
En esta primera activación FeedingSpecialization SOLO modifica:

energía utilizable obtenida de biomasa vegetal ya consumida

NO modifica:

- cantidad de plantas consumidas directamente
- PlantDigestionEfficiency histórica
- MeatAdaptation
- PredatoryDrive
- ScavengingDrive
- elección entre plantas / carroña / caza
- probabilidad de caza
- éxito de caza
- consumo de carroña
- FeedingCapacity
- movimiento
- migración
- reproducción
- mating

PUNTO DE INTEGRACIÓN
--------------------
GrazingSystem mantiene primero:

rawPlantEnergy =
    eaten
    * energyPerFoodUnit
    * plants.EnergyDensity

Después:

usablePlantEnergy =
    rawPlantEnergy
    * PlantDigestionEfficiency
    * FeedingSpecializationPlantEnergyMultiplier

Finalmente:

organism.Eat(usablePlantEnergy)

IMPORTANTE
----------
El organismo consume exactamente la misma biomasa que antes.

El phenotype solo modifica ligeramente cuánta energía obtiene de esa biomasa.

Esto evita confundir:

mayor eficiencia energética

con:

mayor capacidad de extracción / mayor presión directa sobre plantas.

MULTIPLICADOR
-------------
FeedingSpecializationPlantEnergyMultiplier =
    1
    +
    FeedingSpecializationModifier * coupling

Modifier positivo:
- obtiene más energía de la misma biomasa

Modifier negativo:
- obtiene menos energía de la misma biomasa

CONTROL
-------
FeedingSpecializationEcologicalCoupling = 0.0

Entonces:

FeedingSpecializationPlantEnergyMultiplier = 1.0

y el GrazingSystem reproduce exactamente el comportamiento histórico.

EXPERIMENTO
-----------
10 seeds × 3 couplings = 30 corridas.

Seeds:

98765 .. 98774

Couplings:

0.000 -> control
0.125 -> efecto máximo ±1 % sobre energía vegetal utilizable
0.250 -> efecto máximo ±2 % sobre energía vegetal utilizable

MetabolicEfficiency:
0.125

ThermalRegulation:
0.125

BodySize:
0.0

Locomotion:
0.0

SALIDAS
-------
feeding_specialization_parallel_study.csv
feeding_specialization_parallel_summary.csv

MÉTRICAS
--------
Demografía:
- población final
- población mínima y ciclo
- población máxima y ciclo
- racha máxima sin nacimientos
- extinción total
- persistencia regional
- recolonizaciones
- especies

FeedingSpecialization:
- modifier medio final
- modifier medio de padres efectivos
- multiplicador energético vegetal
- multiplicador energético de padres
- PlantDigestionEfficiency
- EffectivePlantEnergyAssimilation
- métricas equivalentes de Llanura

Aislamiento:
- Valle/Bosque MateAccept
- Valle/Llanura MateAccept
- Bosque/Llanura MateAccept

DIAGNÓSTICO NORMAL
------------------
La simulación completa muestra:

FeedingSpecialization vegetal |
Living Avg
Parents Avg
EnergyMult
PlantDig
Assim
Fill
Parents

y valores por región.

Estos diagnósticos:
- no consumen Random
- no modifican la ecología

PROGRAM.CS
----------
CAMBIA.

Se agrega:

--feeding-experiment

Se mantienen:

--locomotion-experiment
--body-size-experiment
--thermal-experiment
--metabolic-experiment

ARCHIVOS A REEMPLAZAR
---------------------
EvolutionSim/Simulation/SimulationConfig.cs
EvolutionSim/Models/PhenotypeEcologyContext.cs
EvolutionSim/Models/Organism.cs
EvolutionSim/Ecology/GrazingSystem.cs
EvolutionSim/Simulation/SimulationEngine.cs
EvolutionSim/Simulation/SimulationStepResult.cs
EvolutionSim/Program.cs

ARCHIVOS NUEVOS
---------------
EvolutionSim/Evolution/FeedingSpecializationSelectionDiagnosticsCalculator.cs
EvolutionSim/Experiments/FeedingSpecializationExperimentResult.cs
EvolutionSim/Experiments/FeedingSpecializationExperimentRunner.cs

PAQUETES / NUGET
----------------
Ninguno.

ORDEN DE VALIDACIÓN
-------------------
1. Compilar:

dotnet build -c Release

2. Ejecutar simulación normal:

dotnet run -c Release --project .\EvolutionSim\EvolutionSim.csproj

Como FeedingSpecialization = 0.0, BodySize = 0.0 y Locomotion = 0.0,
con seed 98765 debe reproducirse el baseline validado:

C1600
Población 1971
Valle 369
Bosque 917
Llanura 685
Especies 2

Además:

EnergyMult = 1.0000

y:

EffectivePlantEnergyAssimilation
=
PlantDigestionEfficiency

3. Solo si el control coincide, ejecutar:

dotnet run -c Release --project .\EvolutionSim\EvolutionSim.csproj -- --feeding-experiment

DECISIÓN
--------
No congelar un coupling por una sola seed.

Comparar:

- extinciones
- persistencia de Llanura
- mínimos poblacionales
- máximos poblacionales
- rachas sin nacimientos
- especies
- modifier padres vs vivos
- multiplicador padres vs vivos
- PlantDigestionEfficiency
- EffectivePlantEnergyAssimilation
- aislamiento reproductivo

Si ±1 % es estable y muestra una señal reproductiva coherente sin dominar
la demografía, será el primer candidato para congelar.

Si ±1 % resulta demasiado sensible, se refina a valores menores.

Si no existe señal consistente incluso cuando cambia la demografía,
FeedingSpecialization permanecerá pasivo.

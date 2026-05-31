# 2026.05.31 ADD ZOYI ZT-DQ02 and ZT-MD2




<img width="770" height="473" alt="z1" src="https://github.com/user-attachments/assets/0becdb55-b46c-4c04-bdea-5b27f093e648" />

How it's works?
```
How the Application Measures ESR (Summary)

The application calculates the Equivalent Series Resistance (ESR) by modeling the real-world capacitor as an ideal capacitor connected in series with a resistor (SER mode).

    AC Signal Injection: The device applies a low-amplitude Alternating Current (AC) voltage signal across the capacitor at a specific test frequency (such as 120 Hz or 100 kHz). The low voltage ensures that nearby semiconductor components on a circuit board do not turn on during an in-circuit test.

    Impedance Measurement: The system measures the resulting current flowing through the capacitor and the phase shift between the voltage and the current waveforms. This allows the application to calculate the total impedance (Z) of the component.

    Mathematical Separation: Using the phase angle (θ) or the dissipation factor (tanδ), the software splits the total impedance into its two functional components: the capacitive reactance (Xc​, which stores energy) and the purely resistive part (Rs​, which dissipates energy as heat).

    ESR Extraction: The isolated resistive value (Rs​) is directly displayed as the real ESR in milliohms (mΩ) or ohms (Ω). Finally, the application compares this reading against pre-programmed manufacturer thresholds (derived from standard tanδ formulas) to determine if the capacitor is healthy or degraded.


```

1. Retrieving Capacity (nominalCapacityUf)

    From tbDQ02UserNominal.Text

    If it contains a suffix (e.g., 100uF, 1nF) → parsed via TryParseUserValue(), the result in Farads is multiplied by 1e6 → µF

    If it is a number only → treated as µF

    If empty → 0 (ESR will not be calculated)

2. Retrieving Frequency (frequency)

    From lblDQ02Freq.Text via ParsujCzestotliwosc():

        "1 kHz" → 1000

        "100 Hz" → 100

    If it is a number only → 120 (default value)

3. Retrieving Voltage (voltage)

    From textBoxvoltage.Text parsed as int

4. Retrieving Temperature (celciusTemp)

    From textBoxtemperature.Text parsed as int

    If empty/invalid → 85°C (default)

5. Retrieving Measured ESR (measuredEsr)

    First, checks parsed.SecondaryValue (field 1 in the DQ02 CSV file)

    If NaN or ≤ 0 → fallback: reads from lblDQ02Secondary.Text stripping out " Ω" and "ESR:"

6. Looking up tanδ from the Database (SzukajTanDeltaZBazy)

    Loads baza_esr.csv (executed once using lazy loading)

    Temperature logic: if ≥95 → treated as 105°C, otherwise 85°C

    From the matching temperature rows: finds the closest voltage match, then the closest capacity match

    If _customTanDelta is set (manually in label6) → it is used instead of the CSV data

    If no close match is found → fallback to ObliczTanDeltaSzczegolowo() (a custom formula based on voltage and temperature)

7. ESR Formula
targetEsr=2⋅π⋅frequency⋅capacityFtanδ​

    where capacityF = nominalCapacityUf / 1_000_000

    Example for 100µF, 100Hz, tanδ=0.12:
    targetEsr=2⋅3.14159⋅100⋅0.00010.12​=0.062830.12​≈1.91 Ω

8. Condition Assessment

    limitWarning = targetEsr * 1.5

    limitReplace = targetEsr * 2.0

    measuredEsr ≤ limitWarning → HEALTHY / OK (green)

    measuredEsr ≤ limitReplace → SLIGHTLY DEGRADED / DRYING OUT (orange)

    remaining cases → FAULTY / DAMAGED (red)





# NADCHODZI WEBSERWER:
<img width="394" height="805" alt="zoyi mobile web" src="https://github.com/user-attachments/assets/a2254827-d2e2-4155-b277-f71407d75f94" />

<img width="1850" height="908" alt="zoyi web" src="https://github.com/user-attachments/assets/cc3613fb-4363-4c64-9637-4261921b97a3" />




<h1 align="center">📋 ZOYI official site</h1>

https://zotektools.com/products/triple-in-one-instrument-combining-oscilloscope-multimeter-and-signal-source/


---

<h1 align="center">📋 ZOYI 703s PC APPLICATION</h1>

---

Ponizej link do glownego zalozyciela projektu:

1.https://github.com/marcinozog/ZOYI-Compatible-Terminal



2.Lista osob ktore wziely udzial przy tym projekcie.

- **Mariam Lopaciński**. To on odpowiada za przebudowanie calej szaty graficznej. Wykonał przepiekną robote za co mu serdecznie dziekuje.

- **Spider web Team** za podeslanie kodu do Advanced display. Jego github z swietna aplikacja dla Anenga i Unit ponizej:

https://github.com/webspiderteam





Ponizej kilka screenow jak wyglada aplikacja.

# GLOWNE OKNO PROGRAMU
<img width="760" height="490" alt="z1" src="https://github.com/user-attachments/assets/bbfcb55b-502a-4879-b226-1ddbab70ce58" />

# OKNO WYKRESU
<img width="772" height="476" alt="z2" src="https://github.com/user-attachments/assets/b3cbfb55-89e8-4eb0-94fb-05ff63c13e55" />

# PLIK CSV PO ZAPISANIU
<img width="1416" height="904" alt="z3" src="https://github.com/user-attachments/assets/26b0dc2e-4ad0-42cd-a9f4-1430eeef8723" />

# USTAWIENIA
<img width="770" height="475" alt="z4" src="https://github.com/user-attachments/assets/db5a0712-914f-4fd2-a51f-4bbfce25f0d0" />

# STANDARDOWY PANEL
<img width="1579" height="130" alt="z5" src="https://github.com/user-attachments/assets/4f360fbc-a519-4a3b-b473-e5d186b71f91" />

# PANEL ZAAWANSOWANY
<img width="486" height="312" alt="z6" src="https://github.com/user-attachments/assets/a6648c8b-9f26-4cfa-89d0-710ddfd4d1d0" />

---

---

Teraz od siebie co ja dodałem do tej wersji :)

- Czytanie pomiaru. Dodana opcja w ustawieniach.
- Wykres pomiaru
- Przy starcie panel zaawansowany sie nie aktywuje.
- Dodanie zapamietywania suwaka od przezroczystosci.
- Stabilizacja pomiaru. Pomiar nie plywa tak jak to bylo na pocztku. Pokazuje stbilne 0.0000 przy braku pomiaru.

- ## UPDATE 2026.05.19

- Dodalem skroty klawiaturowe.
- Poprawilem stabilnosc wykresu.
- Poprawilem zakladke datasheet

- <img width="527" height="508" alt="Zrzut ekranu 2026-05-19 183406" src="https://github.com/user-attachments/assets/e26aa8a7-0bc5-4127-a54c-24fa21ee5fef" />

<img width="794" height="703" alt="z8" src="https://github.com/user-attachments/assets/2c095048-3f68-427e-9c1b-0eb74bc0c0e9" />


















# GOnnect Outlook Add-in

Dieses Add-in ergänzt im klassischen 64-Bit-Outlook für Microsoft 365 das Kontextmenü von
Kontakten um **Mit GOnnect anrufen**. Das Untermenü enthält nur die Rufnummern, die im
markierten Kontakt tatsächlich vorhanden sind.

Beim Anklicken wird die Rufnummer über den registrierten `tel:`-Protokollhandler an GOnnect
übergeben. Die Telefonie selbst, die Kontaktsuche und ein gegebenenfalls konfigurierter
`outgoingDialPrefix` bleiben vollständig Aufgabe von GOnnect.

## Unterstützte Umgebung

- Klassisches Microsoft Outlook für Microsoft 365
- Getestetes Ziel: Version 2608, Build 16.0.20326.20072, 64 Bit
- Windows mit .NET Framework 4.8
- GOnnect als Standardanwendung für `tel:`

Das neue Outlook wird nicht unterstützt, da es keine klassischen COM-Add-ins lädt.

## Installation

1. Outlook vollständig schließen.
2. Das ZIP-Archiv in einen beliebigen Ordner entpacken.
3. `install.cmd` doppelt anklicken.
4. Outlook wieder starten.

Die Installation erfolgt ausschließlich für den aktuellen Windows-Benutzer unter
`%LOCALAPPDATA%\GOnnect\OutlookAddIn` und benötigt keine Administratorrechte.

## Verwendung

Einen einzelnen Kontakt markieren und mit der rechten Maustaste anklicken. Unter
**Mit GOnnect anrufen** die gewünschte Rufnummer auswählen. GOnnect wird über `tel:`
angesprochen und beginnt den Anruf.

Der vorhandene Outlook-Menüpunkt **Anruf** verwendet weiterhin TAPI und wird durch das Add-in
nicht verändert.

## Deinstallation

1. Outlook vollständig schließen.
2. `%LOCALAPPDATA%\GOnnect\OutlookAddIn\uninstall.cmd` ausführen.

## Technische Hinweise

- COM-Klasse: `GOnnect.OutlookAddIn`
- CLSID: `{A3D2629C-32F1-48E7-BD24-AC02E70427E8}`
- Zielplattform: x64 / .NET Framework 4.8
- Outlook-Kontextmenüs: `ContextMenuContactItem` und `ContextMenuFlaggedContactItem`
- Das Add-in enthält keine SIP-Zugangsdaten und kommuniziert nicht selbst mit der FRITZ!Box.


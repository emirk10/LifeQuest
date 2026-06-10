


export function getMoodHint(energyLevel) {
  if (!energyLevel) return "normal";
  if (energyLevel >= 8) return "pozitif";
  if (energyLevel >= 6) return "normal";
  if (energyLevel >= 4) return "yorgun";
  return "zor";
}

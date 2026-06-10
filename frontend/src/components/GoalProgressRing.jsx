import React from 'react';

const GoalProgressRing = ({
  percent = 0,
  size = 80,
  stroke = 7,
  color = '#7c3aed',
  label,
  icon
}) => {
  const radius = (size - stroke) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (percent / 100) * circumference;

  return (
    <div style={{ position: 'relative', width: size, height: size, display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}>
      <svg width={size} height={size} style={{ transform: 'rotate(-90deg)', position: 'absolute' }}>
        {}
        <circle
          cx={size / 2} cy={size / 2} r={radius}
          fill="none"
          stroke="rgba(255,255,255,0.08)"
          strokeWidth={stroke}
        />
        {}
        <circle
          cx={size / 2} cy={size / 2} r={radius}
          fill="none"
          stroke={color}
          strokeWidth={stroke}
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          strokeLinecap="round"
          style={{ transition: 'stroke-dashoffset 0.6s ease' }}
        />
      </svg>
      <div style={{ textAlign: 'center', zIndex: 1 }}>
        {icon
          ? <div style={{ fontSize: size > 70 ? '1.4rem' : '1rem' }}>{icon}</div>
          : <div style={{ fontSize: size > 70 ? '0.95rem' : '0.75rem', fontWeight: 700, color: '#fff' }}>{percent}%</div>
        }
        {label && <div style={{ fontSize: '0.6rem', color: 'rgba(255,255,255,0.45)', marginTop: '2px' }}>{label}</div>}
      </div>
    </div>
  );
};

export default GoalProgressRing;

'use client';

import { useState } from 'react';
import LoginCard from './LoginCard';
import RegisterCard from './RegisterCard';

export default function Login() {
  const [isFlipped, setIsFlipped] = useState(false);

  const handleFlip = () => {
    setIsFlipped(!isFlipped);
  };

  return (
    <div className="w-full max-w-sm mx-auto mt-20">
      <div className="relative w-full perspective-[1000px]">
        <div
          className={`relative [transform-style:preserve-3d] transition-transform duration-700 ${
            isFlipped ? '[transform:rotateY(180deg)]' : ''
          }`}
        >
          <div className="w-full [backface-visibility:hidden]">
            <LoginCard onFlip={handleFlip} />
          </div>
          <div className="absolute top-0 left-0 w-full [backface-visibility:hidden] [transform:rotateY(180deg)]">
            <RegisterCard onFlip={handleFlip} />
          </div>
        </div>
      </div>
    </div>
  );
}

'use client';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import Link from 'next/link';
import { useState } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { useRouter } from 'next/navigation';

interface LoginCardProps {
  onFlip: () => void;
}

export default function LoginCard({ onFlip }: LoginCardProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const { login } = useAuth();
  const router = useRouter();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      await login({ username, password });

      router.push('/');
    } catch (err) {
      setError("Nom d'utilisateur ou mot de passe incorrect");
      console.error('Erreur de connexion:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Card className="w-full">
      <CardHeader className="flex-row items-center justify-between">
        <CardTitle>Connectez-vous à votre compte</CardTitle>
        <button
          type="button"
          onClick={onFlip}
          className="hover:underline text-sm"
          disabled={isLoading}
        >
          S'inscrire
        </button>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit}>
          <div className="flex flex-col gap-6">
            {error && (
              <div className="bg-red-50 text-red-600 p-3 rounded-md text-sm">
                {error}
              </div>
            )}
            <div className="grid gap-2">
              <Label htmlFor="username">Nom d'utilisateur</Label>
              <Input
                id="username"
                type="text"
                placeholder="Nom d'utilisateur"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                disabled={isLoading}
              />
            </div>
            <div className="grid gap-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Mot de passe</Label>
                <Link
                  href="/forgot-password"
                  className="hover:underline text-sm"
                >
                  Mot de passe oublié ?
                </Link>
              </div>
              <Input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                disabled={isLoading}
              />
            </div>
            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? 'Connexion...' : 'Connexion'}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

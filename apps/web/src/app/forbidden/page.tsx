'use client';

import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { useRouter, useSearchParams } from 'next/navigation';
import { ShieldAlert } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';

export default function ForbiddenPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user } = useAuth();

  const attemptedRoute = searchParams.get('route') || 'this page';

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/20">
            <ShieldAlert className="h-8 w-8 text-red-600 dark:text-red-400" />
          </div>
          <CardTitle className="text-2xl">Access Denied</CardTitle>
          <CardDescription>
            You don&apos;t have permission to access {attemptedRoute}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {user && (
            <div className="rounded-lg bg-muted p-4 text-sm">
              <p className="font-medium">Current user:</p>
              <p className="text-muted-foreground">{user.email}</p>
              {user.roles && user.roles.length > 0 && (
                <>
                  <p className="mt-2 font-medium">Your roles:</p>
                  <div className="mt-1 flex flex-wrap gap-1">
                    {user.roles.map((role) => (
                      <span
                        key={role}
                        className="rounded-md bg-background px-2 py-1 text-xs"
                      >
                        {role}
                      </span>
                    ))}
                  </div>
                </>
              )}
            </div>
          )}

          <p className="text-sm text-muted-foreground">
            This page requires specific permissions that your account
            doesn&apos;t have. Please contact an administrator if you believe
            this is an error.
          </p>

          <div className="flex flex-col gap-2">
            <Button
              onClick={() => router.push('/dashboard')}
              className="w-full"
            >
              Go to Dashboard
            </Button>
            <Button
              onClick={() => router.back()}
              variant="outline"
              className="w-full"
            >
              Go Back
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

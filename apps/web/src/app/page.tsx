import Link from 'next/link';
import { Button } from '@/components/ui/button';

export default function Home() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-4">
      <h1 className="text-4xl font-bold">Fluxora</h1>
      <Button variant="outline" asChild>
        <Link href="/login">Login</Link>
      </Button>
    </div>
  );
}

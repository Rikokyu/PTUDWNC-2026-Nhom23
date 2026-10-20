import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

export function EmptyState({
  title,
  description,
  actionLabel,
  actionHref,
}: {
  title: string;
  description?: string;
  actionLabel?: string;
  actionHref?: string;
}) {
  return (
    <Card className="panel center">
      <CardContent>
      <h3>{title}</h3>
      {description ? <p className="muted">{description}</p> : null}
      {actionHref && actionLabel ? (
        <Button asChild className="button-primary">
          <Link href={actionHref}>{actionLabel}</Link>
        </Button>
      ) : null}
      </CardContent>
    </Card>
  );
}

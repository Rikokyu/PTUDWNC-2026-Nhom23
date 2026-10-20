import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";

export function ErrorMessage({
  title,
  message,
  onRetry,
}: {
  title?: string;
  message: string;
  onRetry?: () => void;
}) {
  return (
    <Alert variant="destructive" className="panel-danger">
      <div>
        <AlertTitle>{title ?? "Có lỗi xảy ra"}</AlertTitle>
        <AlertDescription>{message}</AlertDescription>
      </div>
      {onRetry ? (
        <Button type="button" onClick={onRetry}>
          Thử lại
        </Button>
      ) : null}
    </Alert>
  );
}

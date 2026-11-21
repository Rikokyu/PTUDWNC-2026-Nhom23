import { startGoogleSignIn } from "@/app/auth/google/actions";
import { Button } from "@/components/ui/Button";

export function GoogleSignInButton({ enabled, next = "/dashboard" }: { enabled: boolean; next?: string }) {
  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3 text-xs text-content-secondary">
        <span className="h-px flex-1 bg-stone-200" />hoặc tiếp tục với<span className="h-px flex-1 bg-stone-200" />
      </div>
      <form action={startGoogleSignIn}>
        <input type="hidden" name="next" value={next} />
        <Button type="submit" variant="outline" size="lg" className="w-full" disabled={!enabled}
          leftIcon={<span aria-hidden="true" className="flex h-6 w-6 items-center justify-center rounded-full bg-white font-bold text-[#4285f4]">G</span>}>
          Tiếp tục với Google
        </Button>
      </form>
      {!enabled && <p className="text-center text-xs text-content-secondary">Đăng nhập Google tạm thời chưa khả dụng.</p>}
    </div>
  );
}

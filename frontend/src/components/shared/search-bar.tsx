import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

type SearchBarProps = {
  value: string;
  onChange: (value: string) => void;
  onSubmit?: () => void;
  placeholder?: string;
  buttonLabel?: string;
};

export function SearchBar({
  value,
  onChange,
  onSubmit,
  placeholder = "Tìm kiếm món ăn...",
  buttonLabel = "Tìm kiếm",
}: SearchBarProps) {
  return (
    <form
      className="search-bar"
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit?.();
      }}
    >
      <Input
        aria-label="Search recipes"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
      />
      {onSubmit ? (
        <Button type="submit" className="button-primary">
          {buttonLabel}
        </Button>
      ) : null}
    </form>
  );
}

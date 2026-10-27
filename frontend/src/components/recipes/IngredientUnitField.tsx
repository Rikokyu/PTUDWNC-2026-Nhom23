"use client";

import { useEffect, useState } from "react";
import { Input } from "@/components/ui/Input";
import { Select } from "@/components/ui/Select";

const commonUnits = [
  "g",
  "kg",
  "ml",
  "l",
  "muỗng cà phê",
  "muỗng canh",
  "chén",
  "quả",
  "tép",
  "củ",
  "nhánh",
  "lát",
  "gói",
  "miếng",
];

export function IngredientUnitField({
  value,
  onChange,
  id,
}: {
  value: string;
  onChange: (value: string) => void;
  id?: string;
}) {
  const [isCustom, setIsCustom] = useState(
    value !== "" && !commonUnits.includes(value),
  );

  useEffect(() => {
    if (value && !commonUnits.includes(value)) {
      setIsCustom(true);
    }
  }, [value]);

  return (
    <div className="space-y-2">
      <Select
        id={id}
        label="Đơn vị"
        value={isCustom ? "__custom__" : value}
        options={[
          { label: "Chọn đơn vị", value: "" },
          ...commonUnits.map((unit) => ({ label: unit, value: unit })),
          { label: "Khác (nhập đơn vị)", value: "__custom__" },
        ]}
        onChange={(event) => {
          if (event.target.value === "__custom__") {
            setIsCustom(true);
            onChange("");
            return;
          }

          setIsCustom(false);
          onChange(event.target.value);
        }}
      />
      {isCustom && (
        <Input
          label="Đơn vị tùy chỉnh"
          value={value}
          maxLength={50}
          onChange={(event) => onChange(event.target.value)}
          placeholder="Nhập đơn vị đo"
        />
      )}
    </div>
  );
}

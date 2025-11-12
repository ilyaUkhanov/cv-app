<?php

namespace App\ValueObjects;

class SkillItem
{
    public function __construct(
        public string $name,
        public ?SkillLevel $level = null
    ) {}

    public function toArray(): array
    {
        return [
            'name' => $this->name,
            'level' => $this->level?->value,
        ];
    }

    public static function fromArray(array $data): self
    {
        return new self(
            name: $data['name'],
            level: isset($data['level']) ? SkillLevel::from($data['level']) : null
        );
    }
}

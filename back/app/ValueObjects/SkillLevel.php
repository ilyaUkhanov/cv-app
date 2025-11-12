<?php

namespace App\ValueObjects;

enum SkillLevel: string
{
    case BASE = 'base';
    case INTERMEDIAIRE = 'intermediaire';
    case AVANCE = 'avance';
    case EXPERT = 'expert';
}
